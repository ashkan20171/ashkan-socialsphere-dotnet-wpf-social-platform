namespace AshkanSocialSphere.Domain;
public sealed class SocialUser { public Guid Id { get; set; } = Guid.NewGuid(); public string UserName { get; set; } = ""; public string DisplayName { get; set; } = ""; public string Bio { get; set; } = ""; public string AvatarPath { get; set; } = ""; public string CoverPath { get; set; } = ""; public string PasswordHash { get; set; } = ""; public DateTime CreatedUtc { get; set; } = DateTime.UtcNow; }
public sealed class SocialPost { public Guid Id { get; set; } = Guid.NewGuid(); public Guid AuthorId { get; set; } public SocialUser Author { get; set; } = null!; public string Content { get; set; } = ""; public string ImagePath { get; set; } = ""; public DateTime CreatedUtc { get; set; } = DateTime.UtcNow; public List<PostLike> Likes { get; set; } = new(); }
public sealed class PostLike { public Guid PostId { get; set; } public Guid UserId { get; set; } }

public sealed class PostComment { public Guid Id { get; set; } = Guid.NewGuid(); public Guid PostId { get; set; } public Guid UserId { get; set; } public SocialUser User { get; set; } = null!; public string Content { get; set; } = ""; public DateTime CreatedUtc { get; set; } = DateTime.UtcNow; }

public sealed class UserFollow { public Guid FollowerId {get;set;} public Guid FollowingId {get;set;} public DateTime CreatedUtc {get;set;}=DateTime.UtcNow; }

public sealed class DirectMessage { public Guid Id {get;set;}=Guid.NewGuid(); public Guid SenderId {get;set;} public Guid RecipientId {get;set;} public string Content {get;set;}=""; public DateTime CreatedUtc {get;set;}=DateTime.UtcNow; public DateTime? ReadUtc {get;set;} }
public sealed record ConversationSummary(Guid ContactId, string LastMessage, DateTime LastActivityUtc, int UnreadCount);
public sealed class SocialNotification { public bool IsRead {get;set;} = false; public Guid Id {get;set;}=Guid.NewGuid(); public Guid UserId {get;set;} public Guid ActorId {get;set;} public string Kind {get;set;}=""; public string Detail {get;set;}=""; public DateTime CreatedUtc {get;set;}=DateTime.UtcNow; }
