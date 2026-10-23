using AshkanSocialSphere.Application;
using AshkanSocialSphere.Domain;
using Microsoft.EntityFrameworkCore;
namespace AshkanSocialSphere.Infrastructure;
public sealed class SqliteSocialRepository(SocialDbContext db) : ISocialRepository {
 public async Task<SocialUser> UpdateProfileImageAsync(Guid userId,string imagePath,bool cover,CancellationToken ct=default){
  var user=await db.Users.FirstOrDefaultAsync(u=>u.Id==userId,ct)??throw new InvalidOperationException("User not found");
  if(cover)user.CoverPath=imagePath;else user.AvatarPath=imagePath;
  await db.SaveChangesAsync(ct);return user;
 }

 public async Task MarkNotificationReadAsync(Guid notificationId,Guid userId,CancellationToken ct=default) {
  var n=await db.Notifications.FirstOrDefaultAsync(x=>x.Id==notificationId&&x.UserId==userId,ct);
  if(n is null)return; n.IsRead=true; await db.SaveChangesAsync(ct);
 }
 public async Task MarkAllNotificationsReadAsync(Guid userId,CancellationToken ct=default) {
  await db.Notifications.Where(x=>x.UserId==userId&&!x.IsRead).ExecuteUpdateAsync(setters=>setters.SetProperty(x=>x.IsRead,true),ct);
 }

 public async Task<IReadOnlyList<ConversationSummary>> GetConversationSummariesAsync(Guid userId,CancellationToken ct=default){
  var messages=await db.DirectMessages.AsNoTracking().Where(m=>m.SenderId==userId||m.RecipientId==userId)
   .Select(m=>new {m.Id,m.SenderId,m.RecipientId,m.Content,m.CreatedUtc,m.ReadUtc}).ToListAsync(ct);
  return messages.GroupBy(m=>m.SenderId==userId?m.RecipientId:m.SenderId)
   .Select(group=>{var latest=group.OrderByDescending(m=>m.CreatedUtc).ThenByDescending(m=>m.Id).First();
    return new ConversationSummary(group.Key,latest.Content,latest.CreatedUtc,group.Count(m=>m.RecipientId==userId&&m.ReadUtc==null));})
   .OrderByDescending(x=>x.LastActivityUtc).ToArray();
 }
 public async Task MarkConversationReadAsync(Guid userId,Guid contactId,CancellationToken ct=default){
  var readTime=DateTime.UtcNow;
  await db.DirectMessages.Where(m=>m.RecipientId==userId&&m.SenderId==contactId&&m.ReadUtc==null)
   .ExecuteUpdateAsync(setters=>setters.SetProperty(m=>m.ReadUtc,readTime),ct);
 }
 public async Task<IReadOnlyList<DirectMessage>> GetConversationAsync(Guid userId,Guid otherId,CancellationToken ct=default){
  // Fetch the latest 200 messages, then display them chronologically.
  var latest=await db.DirectMessages.AsNoTracking()
   .Where(m=>(m.SenderId==userId&&m.RecipientId==otherId)||(m.SenderId==otherId&&m.RecipientId==userId))
   .OrderByDescending(m=>m.CreatedUtc).ThenByDescending(m=>m.Id).Take(200).ToListAsync(ct);
  latest.Reverse();return latest;
 }
 public async Task SendMessageAsync(Guid senderId,Guid recipientId,string content,CancellationToken ct=default){content=(content??"").Trim();if(content.Length<1||content.Length>2000)throw new ArgumentException("Message must contain 1–2000 characters");if(senderId==recipientId)throw new InvalidOperationException("Choose another user");if(!await db.Users.AnyAsync(x=>x.Id==senderId,ct))throw new InvalidOperationException("Sender not found");if(!await db.Users.AnyAsync(x=>x.Id==recipientId,ct))throw new InvalidOperationException("Recipient not found");db.DirectMessages.Add(new DirectMessage{SenderId=senderId,RecipientId=recipientId,Content=content});db.Notifications.Add(new SocialNotification{UserId=recipientId,ActorId=senderId,Kind="message",Detail="New private message"});await db.SaveChangesAsync(ct);}
 public async Task<IReadOnlyList<SocialNotification>> GetNotificationsAsync(Guid userId,CancellationToken ct=default)=>await db.Notifications.AsNoTracking().Where(n=>n.UserId==userId).OrderByDescending(n=>n.CreatedUtc).Take(50).ToListAsync(ct);


 public async Task<IReadOnlyList<SocialUser>> SearchUsersAsync(string query,Guid currentUserId,CancellationToken ct=default){
  query=(query??"").Trim(); var users=db.Users.AsNoTracking().Where(x=>x.Id!=currentUserId);
  if(query.Length>0) users=users.Where(x=>x.UserName.Contains(query)||x.DisplayName.Contains(query));
  return await users.OrderBy(x=>x.DisplayName).Take(40).ToListAsync(ct);
 }
 public async Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid followerId,CancellationToken ct=default)=>await db.Follows.AsNoTracking().Where(x=>x.FollowerId==followerId).Select(x=>x.FollowingId).ToListAsync(ct);
 public async Task<bool> ToggleFollowAsync(Guid followerId,Guid followingId,CancellationToken ct=default){
  if(followerId==followingId)throw new InvalidOperationException("You cannot follow yourself.");
  if(!await db.Users.AnyAsync(x=>x.Id==followingId,ct))throw new InvalidOperationException("User not found.");
  var existing=await db.Follows.FindAsync(new object[]{followerId,followingId},ct);
  if(existing is null){db.Follows.Add(new UserFollow{FollowerId=followerId,FollowingId=followingId});db.Notifications.Add(new SocialNotification{UserId=followingId,ActorId=followerId,Kind="follow",Detail="Someone followed you"});await db.SaveChangesAsync(ct);return true;}
  db.Follows.Remove(existing);await db.SaveChangesAsync(ct);return false;
 }
 public async Task<bool> ToggleLikeAsync(Guid postId,Guid userId,CancellationToken ct=default){
  if(!await db.Posts.AnyAsync(p=>p.Id==postId,ct))throw new InvalidOperationException("Post not found");
  var like=await db.Likes.FindAsync(new object[]{postId,userId},ct);
  if(like is null){db.Likes.Add(new PostLike{PostId=postId,UserId=userId});var owner=await db.Posts.Where(p=>p.Id==postId).Select(p=>p.AuthorId).FirstAsync(ct);if(owner!=userId)db.Notifications.Add(new SocialNotification{UserId=owner,ActorId=userId,Kind="like",Detail="Someone liked your post"});await db.SaveChangesAsync(ct);return true;}
  db.Likes.Remove(like);await db.SaveChangesAsync(ct);return false;
 }
 public async Task<PostComment> AddCommentAsync(Guid postId,Guid userId,string content,CancellationToken ct=default){
  content=(content??"").Trim();if(content.Length<1||content.Length>500)throw new ArgumentException("Comment must contain 1–500 characters");
  if(!await db.Posts.AnyAsync(p=>p.Id==postId,ct))throw new InvalidOperationException("Post not found");
  var comment=new PostComment{PostId=postId,UserId=userId,Content=content};db.Comments.Add(comment);var owner=await db.Posts.Where(p=>p.Id==postId).Select(p=>p.AuthorId).FirstAsync(ct);if(owner!=userId)db.Notifications.Add(new SocialNotification{UserId=owner,ActorId=userId,Kind="comment",Detail="Someone commented on your post"});await db.SaveChangesAsync(ct);return comment;
 }
 public async Task<IReadOnlyList<PostComment>> GetCommentsAsync(Guid postId,CancellationToken ct=default)=>await db.Comments.AsNoTracking().Include(x=>x.User).Where(x=>x.PostId==postId).OrderBy(x=>x.CreatedUtc).ToListAsync(ct);

 public async Task<SocialUser> GetOrCreateLocalUserAsync(CancellationToken ct = default) { var user = await db.Users.FirstOrDefaultAsync(x=>x.UserName=="local_demo",ct); if(user is not null) return user; user=new SocialUser{UserName="local_demo",DisplayName="Ashkan",Bio="Building connections, one post at a time."}; db.Users.Add(user); await db.SaveChangesAsync(ct); return user; }
 public async Task<SocialUser> UpdateLocalProfileAsync(Guid id,string displayName,string bio,CancellationToken ct=default){ var user=await db.Users.SingleAsync(x=>x.Id==id,ct);user.DisplayName=displayName;user.Bio=bio;await db.SaveChangesAsync(ct);return user; }
 public async Task<IReadOnlyList<SocialPost>> GetFollowingFeedAsync(Guid userId,CancellationToken ct=default) {
  var ids=db.Follows.AsNoTracking().Where(f=>f.FollowerId==userId).Select(f=>f.FollowingId);
  return await db.Posts.AsNoTracking().Include(p=>p.Author).Include(p=>p.Likes)
   .Where(p=>p.AuthorId==userId||ids.Contains(p.AuthorId))
   .OrderByDescending(p=>p.CreatedUtc).Take(100).ToListAsync(ct);
 }
 public Task<SocialUser?> GetPublicProfileAsync(Guid userId,CancellationToken ct=default) =>
  db.Users.AsNoTracking().FirstOrDefaultAsync(u=>u.Id==userId,ct);
 public async Task<IReadOnlyList<SocialPost>> GetUserPostsAsync(Guid userId,CancellationToken ct=default) =>
  await db.Posts.AsNoTracking().Include(p=>p.Author).Include(p=>p.Likes)
   .Where(p=>p.AuthorId==userId).OrderByDescending(p=>p.CreatedUtc).Take(100).ToListAsync(ct);
 public async Task<IReadOnlyList<SocialPost>> GetFeedAsync(CancellationToken ct=default) => await db.Posts.AsNoTracking().Include(x=>x.Author).Include(x=>x.Likes).OrderByDescending(x=>x.CreatedUtc).Take(100).ToListAsync(ct);
 public async Task<bool> UpdateOwnPostAsync(Guid postId,Guid userId,string content,CancellationToken ct=default) {
  content=(content??"").Trim(); if(content.Length>2000)throw new ArgumentException("Post exceeds 2,000 characters.");
  var post=await db.Posts.SingleOrDefaultAsync(x=>x.Id==postId&&x.AuthorId==userId,ct);
  if(post is null)return false;
  if(content.Length==0&&string.IsNullOrWhiteSpace(post.ImagePath))throw new ArgumentException("A post needs text or a photo.");
  post.Content=content;await db.SaveChangesAsync(ct);return true;
 }
 public async Task<bool> UpdateOwnPostWithImagesAsync(Guid postId,Guid userId,string content,string imagePath,CancellationToken ct=default) {
  content=(content??"").Trim(); imagePath??="";
  if(content.Length>2000)throw new ArgumentException("Post exceeds 2,000 characters.");
  if(content.Length==0&&string.IsNullOrWhiteSpace(imagePath))throw new ArgumentException("A post needs text or a photo.");
  var post=await db.Posts.SingleOrDefaultAsync(x=>x.Id==postId&&x.AuthorId==userId,ct);
  if(post is null)return false;
  post.Content=content;post.ImagePath=imagePath;
  await db.SaveChangesAsync(ct);return true;
 }
 public async Task<bool> DeleteOwnPostAsync(Guid postId,Guid userId,CancellationToken ct=default) {
  var post=await db.Posts.SingleOrDefaultAsync(x=>x.Id==postId&&x.AuthorId==userId,ct);
  if(post is null)return false;
  // Explicitly remove dependents so existing databases do not rely on cascade migrations.
  var comments=await db.Comments.Where(x=>x.PostId==postId).ToListAsync(ct);
  var likes=await db.Likes.Where(x=>x.PostId==postId).ToListAsync(ct);
  db.Comments.RemoveRange(comments);db.Likes.RemoveRange(likes);db.Posts.Remove(post);
  await db.SaveChangesAsync(ct);return true;
 }
 public async Task<SocialPost> AddPostAsync(Guid authorId,string content,string imagePath="",CancellationToken ct=default) { content=(content??"").Trim();imagePath??="";if((string.IsNullOrWhiteSpace(content)&&string.IsNullOrWhiteSpace(imagePath))||content.Length>2000) throw new ArgumentException("Post must contain 1–2000 characters."); var post=new SocialPost{AuthorId=authorId,Content=content,ImagePath=imagePath}; db.Posts.Add(post); await db.SaveChangesAsync(ct); return post; }
}
