using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AshkanSocialSphere.Application;
using AshkanSocialSphere.Domain;
namespace AshkanSocialSphere.Desktop.ViewModels;
public sealed class MainViewModel : INotifyPropertyChanged {
 private readonly ISocialRepository _repo; private SocialUser? _user; private string _draft=""; private string _status=""; private string _page="Feed"; private bool _persian; private bool _light; private readonly string _preferencesFile=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AshkanSocialSphere","preferences.json"); private string _selectedPostImage=""; public string SelectedPostImage {get=>_selectedPostImage;private set{_selectedPostImage=value;Notify();}} public ObservableCollection<string> SelectedPostImages {get;}=new(); public ICommand ChoosePostImageCommand {get;} public ICommand ClearPostImageCommand {get;} private string _profileName=""; private string _profileBio="";
 public event PropertyChangedEventHandler? PropertyChanged;
 private void Notify([CallerMemberName] string? name=null)=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
 public ObservableCollection<PostItem> Posts {get;}=new(); public string Draft {get=>_draft;set{_draft=value;Notify();Notify(nameof(DraftCharacterCount));Notify(nameof(CanPublish));}} public string Status {get=>_status;set{_status=value;Notify();}} public string Page {get=>_page;set{_page=value;Notify();Notify(nameof(PageTitle));Notify(nameof(IsFeed));Notify(nameof(IsOtherPage));Notify(nameof(IsProfile));Notify(nameof(IsExplore));Notify(nameof(IsPlaceholder));Notify(nameof(IsSettings));Notify(nameof(IsPeople));Notify(nameof(IsPublicProfile));Notify(nameof(IsMessages));Notify(nameof(IsNotifications));}} public string PageTitle => _persian ? Page switch {"Feed"=>"خانه و پست‌ها","Explore"=>"کاوش","Messages"=>"پیام‌ها","People"=>"افراد","Notifications"=>"اعلان‌ها","PublicProfile"=>"پروفایل عمومی","Profile"=>"پروفایل",_=>"تنظیمات"} : Page switch {"Feed"=>"Your feed","Explore"=>"Explore","Messages"=>"Messages","People"=>"People","Notifications"=>"Notifications","PublicProfile"=>"Public profile","Profile"=>"Profile",_=>"Settings"}; public bool IsMessages=>Page=="Messages"; public bool IsNotifications=>Page=="Notifications"; public bool IsPeople=>Page=="People"; public bool IsPublicProfile=>Page=="PublicProfile"; public bool IsFeed=>Page=="Feed"; public bool IsProfile=>Page=="Profile"; public bool IsSettings=>Page=="Settings"; public bool IsExplore=>Page=="Explore"; public bool IsPlaceholder=>false; public bool IsOtherPage=>IsPlaceholder; public bool Persian {get=>_persian;set{_persian=value;Notify();Notify(nameof(PageTitle));Notify(nameof(Welcome));Notify(nameof(Placeholder));Notify(nameof(ComposeLabel));Notify(nameof(PublishLabel));Notify(nameof(LanguageLabel));Notify(nameof(ComingSoon));Notify(nameof(PeopleLabel));Notify(nameof(SearchLabel));Notify(nameof(FollowLabel));Notify(nameof(FeedLabel));Notify(nameof(ExploreLabel));Notify(nameof(MessagesLabel));Notify(nameof(NotificationsLabel));Notify(nameof(SendLabel));Notify(nameof(RecipientLabel));Notify(nameof(RefreshConversationLabel));Notify(nameof(RefreshContactListLabel));Notify(nameof(RefreshNotificationsLabel));Notify(nameof(MessageSearchLabel));Notify(nameof(ContactSearchLabel));Notify(nameof(NoContactsLabel));Notify(nameof(SelectedContactLabel));Notify(nameof(LastMessageLabel));Notify(nameof(MarkAllReadLabel));Notify(nameof(MarkReadLabel));Notify(nameof(ProfileLabel));Notify(nameof(SettingsLabel));Notify(nameof(SidebarCaption));Notify(nameof(CommunityTitle));Notify(nameof(CommunityDescription));Notify(nameof(NextTitle));Notify(nameof(NextDescription));Notify(nameof(ComposeHint));Notify(nameof(IsRtl));Notify(nameof(LayoutDirection));Notify(nameof(ProfileHeading));Notify(nameof(NameLabel));Notify(nameof(BioLabel));Notify(nameof(SaveProfileLabel));Notify(nameof(PostCountLabel));Notify(nameof(ExploreHeading));Notify(nameof(ExploreDescription));Notify(nameof(SettingsHeading));Notify(nameof(SettingsDescription));Notify(nameof(ThemeSettingLabel));Notify(nameof(LanguageSettingLabel));Notify(nameof(EmptyFeedLabel));Notify(nameof(FollowingFeedLabel));Notify(nameof(EveryoneFeedLabel));Notify(nameof(PublicProfileHeading));Notify(nameof(PublicPostsHeading));Notify(nameof(PostsMetricLabel));Notify(nameof(MembersMetricLabel));Notify(nameof(UnreadMetricLabel));Notify(nameof(LikeActionLabel));Notify(nameof(CommentActionLabel));Notify(nameof(CommentPlaceholderLabel));Notify(nameof(CommentsHeadingLabel));Notify(nameof(SignOutLabel));Notify(nameof(LocalModeLabel));Notify(nameof(AddPhotoLabel));Notify(nameof(RemovePhotoLabel));Notify(nameof(ChangeAvatarLabel));Notify(nameof(ChangeCoverLabel));Notify(nameof(ProfilePhotosLabel));Notify(nameof(EditOwnPostLabel));Notify(nameof(DeleteOwnPostLabel));Notify(nameof(MyPostsHeading));Notify(nameof(MyPhotosHeading));Notify(nameof(EmptyMyPostsLabel));Notify(nameof(EmptyMyPhotosLabel));Notify(nameof(MyPostCountLabel));Notify(nameof(MyPhotoCountLabel));Notify(nameof(PreviewImageHint));Notify(nameof(PostContentHint));Notify(nameof(PhotoLimitLabel));Notify(nameof(MessageLengthLabel));Notify(nameof(UnreadMessagesLabel));Notify(nameof(ConversationHeading));Notify(nameof(EmptyConversationLabel));Notify(nameof(UnreadCountLabel));Notify(nameof(EmptyNotificationsLabel));LocalizeNotifications();SavePreferences();}} public string Welcome=>Persian?"به دنیای SocialSphere خوش آمدید":"Welcome back to SocialSphere"; public string Placeholder=>Persian?"چه چیزی در ذهن دارید؟":"What's on your mind?"; public string ComposeLabel=>Persian?"پست جدید":"Create a post"; public string PublishLabel=>Persian?"انتشار پست":"Publish post"; public string LanguageLabel=>Persian?"English / فارسی":"فارسی / English"; public string ComingSoon=>Persian?"این بخش در فازهای بعدی فعال می‌شود.":"This module is scheduled for a later phase.";
 public bool IsRtl=>Persian; public System.Windows.FlowDirection LayoutDirection=>Persian?System.Windows.FlowDirection.RightToLeft:System.Windows.FlowDirection.LeftToRight;
 public string PeopleLabel=>Persian?"♧   کاربران":"♧   People"; public string SearchLabel=>Persian?"جست‌وجوی کاربران":"Search users"; public string FollowLabel=>Persian?"دنبال کردن / لغو دنبال کردن":"Follow / Unfollow";
 public string FeedLabel=>Persian?"⌂   خانه":"⌂   Feed";
 public string ExploreLabel=>Persian?"◈   کاوش":"◈   Explore";
 public string NotificationsLabel=>Persian?"🔔   اعلان‌ها":"🔔   Notifications"; public string MessagesLabel=>Persian?"✉   پیام‌ها":"✉   Messages";
 public string ProfileLabel=>Persian?"♙   پروفایل":"♙   Profile";
 public string SettingsLabel=>Persian?"⚙   تنظیمات":"⚙   Settings";
 public string SidebarCaption=>Persian?"نسخه پنجاهم • شبکه اجتماعی محلی":"PHASE 50 • LOCAL SOCIAL NETWORK";
 public string CommunityTitle=>Persian?"جامعه کاربران":"COMMUNITY";
 public string CommunityDescription=>Persian?"فضای محلی شما برای ساخت و اشتراک‌گذاری پست‌ها":"Your private local space for creating and sharing posts.";
 public string NextTitle=>Persian?"وضعیت پلتفرم":"PLATFORM STATUS";
 public string NextDescription=>Persian?"• پیام‌های خصوصی محلی\n\n• اعلان‌های رویداد\n\n• اتصال سرور در آینده":"• Local private messages\n\n• Activity notifications\n\n• Server integration planned";
 public string ComposeHint=>Persian?"پست متنی • حداکثر ۲۰۰۰ نویسه":"Text posts • up to 2,000 characters";
 public string ThemeLabel=>_light?(Persian?"حالت تاریک":"Dark mode"):(Persian?"حالت روشن":"Light mode");
 public ICommand ToggleThemeCommand {get;}
 private void ToggleTheme(){_light=!_light; var r=System.Windows.Application.Current.Resources;
 r["BackgroundBrush"]=new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_light?"#F3F5FC":"#0B1020"));
 r["PanelBrush"]=new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_light?"#FFFFFF":"#151D34"));
 r["TextBrush"]=new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_light?"#1B2440":"#F5F7FF"));
 r["MutedBrush"]=new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_light?"#5C6880":"#9AA7C2"));
 r["SidebarBrush"]=new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_light?"#E8ECFA":"#10182B"));
 r["InputBrush"]=new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_light?"#F6F7FC":"#202A45"));
 Notify(nameof(ThemeLabel));SavePreferences();}

 public string ProfileName {get=>_profileName;set{_profileName=value;Notify();}}
 public string ProfileBio {get=>_profileBio;set{_profileBio=value??"";Notify();Notify(nameof(ProfileBioLengthLabel));}}
 public string ProfileBioLengthLabel => $"{ProfileBio.Length}/500";
 public string ProfileHeading=>Persian?"ویرایش پروفایل محلی":"Edit your local profile";
 public string NameLabel=>Persian?"نام نمایشی":"Display name";
 public string BioLabel=>Persian?"درباره من":"Biography";
 public string SaveProfileLabel=>Persian?"ذخیره پروفایل":"Save profile";
 public string EditOwnPostLabel=>Persian?"ویرایش":"Edit";
 public string DeleteOwnPostLabel=>Persian?"حذف":"Delete";
 public async Task<bool> EditOwnPostAsync(Guid id,string content) {
  if(_user is null)return false;
  try { var ok=await _repo.UpdateOwnPostAsync(id,_user.Id,content); if(ok){await RefreshAsync();Status=Persian?"پست ویرایش شد":"Post updated";} return ok; }
  catch(Exception){Status=Persian?"ویرایش پست انجام نشد":"Could not update the post";return false;}
 }
 public async Task<bool> EditOwnPostWithImagesAsync(Guid id,string content,IReadOnlyList<string> images) {
  if(_user is null)return false;
  try {
   if(images.Count>4){Status=Persian?"حداکثر چهار تصویر مجاز است":"Maximum four photos";return false;}
   var ok=await _repo.UpdateOwnPostWithImagesAsync(id,_user.Id,content,PostImageCodec.Encode(images));
   if(ok){await RefreshAsync();Status=Persian?"پست و تصاویر به‌روز شدند":"Post and photos updated";}
   return ok;
  }catch(Exception){Status=Persian?"ویرایش پست انجام نشد":"Could not update the post";return false;}
 }
 public async Task<bool> DeleteOwnPostAsync(Guid id) {
  if(_user is null)return false;
  try { var ok=await _repo.DeleteOwnPostAsync(id,_user.Id); if(ok){await RefreshAsync();Status=Persian?"پست حذف شد":"Post deleted";} return ok; }
  catch(Exception){Status=Persian?"حذف پست انجام نشد":"Could not delete the post";return false;}
 }
 public ObservableCollection<PostItem> MyPosts {get;}=new();
 public ObservableCollection<string> MyPhotos {get;}=new();
 public string MyPostsHeading=>Persian?"پست‌های من":"My posts";
 public string MyPhotosHeading=>Persian?"گالری تصاویر من":"My photo gallery";
 public string EmptyMyPostsLabel=>Persian?"هنوز پستی منتشر نکرده‌اید.":"You have not published any posts yet.";
 public string EmptyMyPhotosLabel=>Persian?"هنوز تصویری در پست‌های شما وجود ندارد.":"No photos in your posts yet.";
 public bool HasNoMyPosts=>MyPosts.Count==0;
 public bool HasNoMyPhotos=>MyPhotos.Count==0;
 public string MyPostCountLabel=>Persian?$"{MyPosts.Count} پست":$"{MyPosts.Count} posts";
 public string MyPhotoCountLabel=>Persian?$"{MyPhotos.Count} تصویر":$"{MyPhotos.Count} photos";
 public string PostCountLabel=>Persian?$"تعداد پست‌ها: {MyPosts.Count}":$"Posts published: {MyPosts.Count}";
 private async Task RefreshMyPostsAsync(){
  if(_user is null)return;
  var own=await _repo.GetUserPostsAsync(_user.Id);
  MyPosts.Clear();MyPhotos.Clear();
  foreach(var post in own){var item=ToPostItem(post);MyPosts.Add(item);foreach(var photo in item.Images)MyPhotos.Add(photo);}
  Notify(nameof(HasNoMyPosts));Notify(nameof(HasNoMyPhotos));Notify(nameof(PostCountLabel));Notify(nameof(MyPostCountLabel));Notify(nameof(MyPhotoCountLabel));
 }
 public string SettingsHeading=>Persian?"شخصی‌سازی ظاهر":"Appearance preferences"; public string SettingsDescription=>Persian?"تنظیمات روی همین دستگاه ذخیره می‌شوند.":"Preferences are saved on this device."; public string ThemeSettingLabel=>Persian?"تغییر حالت روشن / تاریک":"Switch light / dark mode"; public string LanguageSettingLabel=>Persian?"تغییر زبان فارسی / انگلیسی":"Switch Persian / English"; public string EmptyFeedLabel=>Persian?"هنوز پستی وجود ندارد. اولین پست را منتشر کنید.":"No posts yet. Share the first update."; public string ExploreHeading=>Persian?"جدیدترین پست‌ها":"Latest community posts";
 public string ExploreDescription=>Persian?"مرور پست‌های ذخیره‌شده در این دستگاه":"Browse posts stored on this device";
 public bool FollowingOnly {get=>_followingOnly;set{if(_followingOnly==value)return;_followingOnly=value;Notify();_=RefreshSafelyAsync();}} private bool _followingOnly;
 public string FollowingFeedLabel=>Persian?"دنبال‌شوندگان":"Following";
 public string EveryoneFeedLabel=>Persian?"همه پست‌ها":"Everyone";
 public string PublicProfileHeading=>Persian?"پروفایل عمومی":"Public profile";
 public string PublicPostsHeading=>Persian?"پست‌های این کاربر":"Posts by this member";
 public string PublicDisplayName {get=>_publicDisplayName;private set{_publicDisplayName=value;Notify();}} private string _publicDisplayName="";
 public string PublicUserName {get=>_publicUserName;private set{_publicUserName=value;Notify();}} private string _publicUserName="";
 public string PublicBio {get=>_publicBio;private set{_publicBio=value;Notify();}} private string _publicBio="";
 public string AvatarPath {get=>_avatarPath;private set{_avatarPath=value;Notify();}} private string _avatarPath="";
 public string CoverPath {get=>_coverPath;private set{_coverPath=value;Notify();}} private string _coverPath="";
 public string PublicAvatarPath {get=>_publicAvatarPath;private set{_publicAvatarPath=value;Notify();}} private string _publicAvatarPath="";
 public string PublicCoverPath {get=>_publicCoverPath;private set{_publicCoverPath=value;Notify();}} private string _publicCoverPath="";
 public ICommand ChangeAvatarCommand {get;}
 public ICommand ChangeCoverCommand {get;}
 private async Task ChooseImageAsync(bool cover){
  if(_user is null)return;
  var picker=new Microsoft.Win32.OpenFileDialog { Title=cover?"Choose cover image":"Choose avatar",Filter="Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",CheckFileExists=true,Multiselect=false};
  if(picker.ShowDialog()!=true)return;
  try{
   var source=new System.IO.FileInfo(picker.FileName);
   if(source.Length>5*1024*1024)throw new InvalidOperationException("Image must be 5 MB or smaller.");
   // Decode the image before storing; a renamed arbitrary file is not accepted.
   using(var stream=source.OpenRead()){
    var decoder=System.Windows.Media.Imaging.BitmapDecoder.Create(stream,System.Windows.Media.Imaging.BitmapCreateOptions.None,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
    if(decoder.Frames.Count==0||decoder.Frames[0].PixelWidth>8000||decoder.Frames[0].PixelHeight>8000)throw new InvalidOperationException("Invalid image dimensions.");
   }
   var folder=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AshkanSocialSphere","media",_user.Id.ToString("N"));
   System.IO.Directory.CreateDirectory(folder);
   var extension=System.IO.Path.GetExtension(source.Name).ToLowerInvariant();
   var destination=System.IO.Path.Combine(folder,(cover?"cover_":"avatar_")+Guid.NewGuid().ToString("N")+extension);
   System.IO.File.Copy(source.FullName,destination);
   try{_user=await _repo.UpdateProfileImageAsync(_user.Id,destination,cover);}
   catch{System.IO.File.Delete(destination);throw;}
   if(cover)CoverPath=destination;else AvatarPath=destination;
   Status=Persian?"تصویر ذخیره شد":"Image saved";
  }catch(Exception ex){Status=ex.Message;}
 }
 public ObservableCollection<PostItem> PublicPosts {get;}=new();
 public ObservableCollection<PersonItem> MessageContacts {get;}=new();
 private string _contactQuery="";
 public string ContactQuery {get=>_contactQuery;set{_contactQuery=value??"";Notify();FilterMessageContacts();}}
 public string ContactSearchLabel=>Persian?"جست‌وجوی مخاطب":"Find a contact";
 public string NoContactsLabel=>Persian?"مخاطبی یافت نشد. از بخش افراد، کاربران را پیدا کنید.":"No contacts found. Discover people in the People tab.";
 public bool HasNoContacts=>MessageContacts.Count==0;
 public string UnreadMessagesLabel=>Persian?$"پیام‌های خوانده‌نشده: {TotalUnreadMessages}":$"Unread messages: {TotalUnreadMessages}";
 public int TotalUnreadMessages=>People.Sum(p=>p.UnreadMessages);
 public string LastMessageLabel=>RecipientId is null ? (Persian?"هنوز گفت‌وگویی انتخاب نشده":"No conversation selected") : (_lastMessagePreview.Length==0?(Persian?"هنوز پیامی وجود ندارد":"No messages yet"):_lastMessagePreview);
 private string _lastMessagePreview="";
 public string SelectedContactLabel=>RecipientId is Guid id
  ? (Persian?"گفت‌وگو با ":"Conversation with ")+(People.FirstOrDefault(p=>p.Id==id)?.DisplayName??(Persian?"کاربر":"user"))
  : (Persian?"یک مخاطب انتخاب کنید":"Choose a contact");
 private void FilterMessageContacts(){
  var selected=RecipientId;
  var matches=People.OrderByDescending(p=>p.LastActivityUtc).ThenBy(p=>p.DisplayName,StringComparer.OrdinalIgnoreCase).Where(p=>string.IsNullOrWhiteSpace(ContactQuery)
   ||p.DisplayName.Contains(ContactQuery,StringComparison.OrdinalIgnoreCase)
   ||p.UserName.Contains(ContactQuery,StringComparison.OrdinalIgnoreCase)).ToArray();
  MessageContacts.Clear();foreach(var person in matches)MessageContacts.Add(person);
  Notify(nameof(HasNoContacts));Notify(nameof(SelectedContactLabel));
  // Preserve the active conversation even if its contact is filtered out.
  if(selected is not null && MessageContacts.All(p=>p.Id!=selected.Value))
   Notify(nameof(SelectedContactLabel));
 }
 public ObservableCollection<MessageItem> Conversation {get;}=new(); public ObservableCollection<NotificationItem> Notifications {get;}=new();
 public ObservableCollection<MessageItem> VisibleConversation {get;}=new(); public bool HasNoConversation=>VisibleConversation.Count==0;
 private string _messageSearch=""; public string MessageSearch {get=>_messageSearch;set{_messageSearch=value;Notify();FilterConversation();}}
 private void FilterConversation(){VisibleConversation.Clear();foreach(var m in Conversation.Where(x=>string.IsNullOrWhiteSpace(MessageSearch)||x.Content.Contains(MessageSearch,StringComparison.OrdinalIgnoreCase)))VisibleConversation.Add(m);Notify(nameof(HasNoConversation));}
 public string MessageSearchLabel=>Persian?"جست‌وجو در گفتگو":"Search this conversation";
 public string MarkAllReadLabel=>Persian?"خواندن همه":"Mark all as read";
 public string MarkReadLabel=>Persian?"خوانده شد":"Mark as read";
 public int UnreadCount=>Notifications.Count(x=>!x.IsRead);
 public string UnreadCountLabel=>Persian?$"خوانده‌نشده: {UnreadCount}":$"Unread: {UnreadCount}";
 public string EmptyNotificationsLabel=>Persian?"هنوز اعلانی ندارید.":"No notifications yet.";
 public bool HasNoNotifications=>Notifications.Count==0;
 private void LocalizeNotifications(){foreach(var item in Notifications)item.UpdateLanguage(Persian);}

 public int VisiblePostCount=>Posts.Count;
 public int CommunityMemberCount=>People.Count;
 public string PostsMetricLabel=>Persian?"پست‌های فید":"Feed posts";
 public string MembersMetricLabel=>Persian?"کاربران جامعه":"Community members";
 public string UnreadMetricLabel=>Persian?"اعلان‌های جدید":"Unread alerts";
 public string LikeActionLabel=>Persian?"♥ پسندیدن / لغو":"♥ Like / Unlike";
 public string CommentActionLabel=>Persian?"ثبت نظر":"Comment";
 public string CommentPlaceholderLabel=>Persian?"نظر خود را بنویسید (حداکثر ۵۰۰ نویسه)":"Write a comment (up to 500 characters)";
 public string CommentsHeadingLabel=>Persian?"گفت‌وگوها":"Discussion";
 public string SignOutLabel=>Persian?"خروج از حساب":"Sign out";
 public string LocalModeLabel=>Persian?"حالت آفلاین • داده‌های محلی":"OFFLINE • LOCAL DATA";
 public string DraftCharacterCount => $"{Draft.Length:N0} / 2,000";
 public bool CanPublish => !string.IsNullOrWhiteSpace(Draft) || SelectedPostImages.Count>0;
 public string PostContentHint => Persian ? "متن یا عکس برای انتشار کافی است" : "Share text, a photo, or both";
 public string AddPhotoLabel=>Persian?"📷 افزودن تصاویر":"📷 Add photos";
 public string PhotoLimitLabel=>Persian?$"{SelectedPostImages.Count} از ۴ تصویر انتخاب شده":$"{SelectedPostImages.Count} of 4 photos selected";
 public string RemovePhotoLabel=>Persian?"✕ حذف تصویر":"✕ Remove photo";
 public string ChangeAvatarLabel=>Persian?"تغییر آواتار":"Change avatar";
 public string ChangeCoverLabel=>Persian?"تغییر کاور":"Change cover";
 public string ProfilePhotosLabel=>Persian?"تصاویر پروفایل":"Profile photos";
 public string PreviewImageHint=>Persian?"برای نمایش بزرگ روی تصویر کلیک کنید":"Click an image to preview";
 public ICommand MarkReadCommand {get;} public ICommand MarkAllReadCommand {get;}
 private async Task MarkReadAsync(object? arg){if(_user is null||arg is not NotificationItem item||item.IsRead)return;try{await _repo.MarkNotificationReadAsync(item.Id,_user.Id);await RefreshNotificationsAsync();}catch(Exception){Status=Persian?"به‌روزرسانی اعلان ناموفق بود.":"Could not update notification.";}}
 private async Task MarkAllReadAsync(){if(_user is null||UnreadCount==0)return;try{await _repo.MarkAllNotificationsReadAsync(_user.Id);await RefreshNotificationsAsync();}catch(Exception){Status=Persian?"به‌روزرسانی اعلان‌ها ناموفق بود.":"Could not update notifications.";}}

 private Guid? _recipientId; public Guid? RecipientId {get=>_recipientId;set{if(_recipientId==value)return;_recipientId=value;_lastMessagePreview="";Notify();Notify(nameof(SelectedContactLabel));Notify(nameof(LastMessageLabel));_=LoadConversationAsync();}}
 private string _messageDraft=""; public string MessageDraft {get=>_messageDraft;set{_messageDraft=value;Notify();Notify(nameof(MessageLengthLabel));}}
 public string MessageLengthLabel=>Persian?$"{MessageDraft.Length} از ۲۰۰۰ نویسه":$"{MessageDraft.Length} / 2000 characters";
 public string ConversationHeading=>Persian?"گفت‌وگوی خصوصی":"PRIVATE CONVERSATION";
 public string EmptyConversationLabel=>Persian?"برای شروع، مخاطبی را انتخاب کنید و پیام بفرستید.":"Select a contact and send a message to begin.";
 private bool _sendingMessage;
 private bool _refreshingChat;
 private int _conversationVersion;
 public string RefreshConversationLabel=>Persian?"تازه‌سازی گفتگو":"Refresh conversation"; public string RefreshNotificationsLabel=>Persian?"تازه‌سازی اعلان‌ها":"Refresh notifications";
 public string RefreshContactListLabel=>Persian?"تازه‌سازی فهرست گفتگوها":"Refresh conversations";
 public ICommand RefreshContactListCommand {get;}
 private async Task RefreshContactListAsync(){await RefreshConversationSummariesAsync();}
 // Periodic updates only while Messages is visible; preserve drafts and selected contact.
 public async Task RefreshChatInBackgroundAsync(){
  if(_user is null || !IsMessages || _refreshingChat)return;
  _refreshingChat=true;
  try {
   await RefreshConversationSummariesAsync();
   if(RecipientId is not null && IsMessages)await LoadConversationAsync();
  } finally { _refreshingChat=false; }
 }
 public string RecipientLabel=>Persian?"گیرنده پیام":"Message recipient"; public string SendLabel=>Persian?"ارسال پیام":"Send message";
 public ICommand SendMessageCommand {get;} public ICommand RefreshMessagesCommand {get;} public ICommand RefreshNotificationsCommand {get;}
 private async Task LoadConversationAsync(){
  var version=++_conversationVersion;
  if(_user is null||RecipientId is null){Conversation.Clear();VisibleConversation.Clear();Notify(nameof(HasNoConversation));_lastMessagePreview="";Notify(nameof(LastMessageLabel));return;}
  var recipient=RecipientId.Value;
  try{
   var items=await _repo.GetConversationAsync(_user.Id,recipient);
   if(version!=_conversationVersion||RecipientId!=recipient)return;
   Conversation.Clear();foreach(var m in items)Conversation.Add(new MessageItem(m.SenderId==_user.Id?ProfileName:"←",m.Content,m.CreatedUtc.ToLocalTime().ToString("g"),m.SenderId==_user.Id, m.SenderId!=_user.Id ? "" : (m.ReadUtc.HasValue ? (Persian ? "خوانده‌شده ✓✓" : "Read ✓✓") : (Persian ? "ارسال‌شده ✓" : "Sent ✓"))));
   FilterConversation();
   var latest=items.OrderByDescending(m=>m.CreatedUtc).FirstOrDefault();
   _lastMessagePreview=latest is null?"":latest.Content;
   Notify(nameof(LastMessageLabel));
   var contact=People.FirstOrDefault(p=>p.Id==recipient);
   if(contact is not null){contact.LastActivityUtc=latest?.CreatedUtc??DateTime.MinValue;FilterMessageContacts();}
   if(version!=_conversationVersion || RecipientId!=recipient)return;
   await _repo.MarkConversationReadAsync(_user.Id,recipient);
   if(version==_conversationVersion&&RecipientId==recipient)await RefreshConversationSummariesAsync();
  }catch(Exception){if(version==_conversationVersion)Status=Persian?"بارگذاری گفتگو انجام نشد.":"Unable to load conversation.";}
 }
 private async Task RefreshConversationSummariesAsync(){
  if(_user is null)return;
  try{
   var summaries=await _repo.GetConversationSummariesAsync(_user.Id);
   var lookup=summaries.ToDictionary(x=>x.ContactId);
   foreach(var person in People){
    if(lookup.TryGetValue(person.Id,out var summary)){
     person.LastActivityUtc=summary.LastActivityUtc;
     person.UnreadMessages=summary.UnreadCount;
     person.LastMessagePreview=summary.LastMessage;
    }else{person.LastActivityUtc=DateTime.MinValue;person.UnreadMessages=0;person.LastMessagePreview="";}
   }
   FilterMessageContacts();Notify(nameof(TotalUnreadMessages));Notify(nameof(UnreadMessagesLabel));
  }catch(Exception){Status=Persian?"بارگذاری وضعیت پیام‌ها ناموفق بود.":"Unable to load message status.";}
 }
 private async Task SendMessageAsync(){
  if(_sendingMessage)return;
  if(_user is null||RecipientId is null){Status=Persian?"گیرنده را انتخاب کنید":"Select a recipient";return;}
  var content=MessageDraft.Trim();
  if(content.Length is < 1 or > 2000){Status=Persian?"پیام باید بین ۱ تا ۲۰۰۰ نویسه باشد.":"Message must contain 1–2000 characters.";return;}
  var recipient=RecipientId.Value;
  _sendingMessage=true;
  try{
   await _repo.SendMessageAsync(_user.Id,recipient,content);
   // Do not erase text the user edited while the send operation was running.
   if(MessageDraft.Trim()==content)MessageDraft="";
   if(RecipientId==recipient)await LoadConversationAsync();
   Status=Persian?"پیام ارسال شد.":"Message sent.";
  }catch(Exception){Status=Persian?"ارسال پیام ناموفق بود؛ متن شما حفظ شد.":"Message could not be sent; your draft was kept.";}
  finally{_sendingMessage=false;}
 }
 private async Task RefreshNotificationsAsync(){if(_user is null)return;try{var items=await _repo.GetNotificationsAsync(_user.Id);Notifications.Clear();foreach(var n in items)Notifications.Add(new NotificationItem(n.Id,n.Kind,n.Detail,n.CreatedUtc.ToLocalTime().ToString("g"),n.IsRead,Persian));Notify(nameof(UnreadCount));Notify(nameof(UnreadCountLabel));Notify(nameof(HasNoNotifications));}catch(Exception){Status=Persian?"دریافت اعلان‌ها ناموفق بود.":"Could not load notifications.";}}
 public ICommand OpenPublicProfileCommand {get;}
 private async Task OpenPublicProfileAsync(object? parameter){
  if(parameter is not PersonItem person)return;
  try {var profile=await _repo.GetPublicProfileAsync(person.Id);if(profile is null){Status="User not found";return;}
   PublicDisplayName=profile.DisplayName;PublicUserName="@"+profile.UserName;PublicBio=profile.Bio;PublicAvatarPath=profile.AvatarPath;PublicCoverPath=profile.CoverPath;
   PublicPosts.Clear();foreach(var p in await _repo.GetUserPostsAsync(person.Id))PublicPosts.Add(ToPostItem(p));
   Page="PublicProfile";
  }catch(Exception ex){Status=ex.Message;}
 }
 private PostItem ToPostItem(SocialPost p)=>new(p.Id,p.Author.DisplayName,p.Content,p.CreatedUtc.ToLocalTime().ToString("g"),p.Likes.Count,_user is not null&&p.Likes.Any(l=>l.UserId==_user.Id),p.ImagePath);
 private async Task RefreshSafelyAsync(){try{await RefreshAsync();}catch(Exception ex){Status=ex.Message;}}
 public ObservableCollection<PersonItem> People {get;}=new(); private string _userQuery=""; public string UserQuery {get=>_userQuery;set{_userQuery=value;Notify();}} public ICommand SearchPeopleCommand {get;} public ICommand ToggleFollowCommand {get;}
 private async Task SearchPeopleAsync(){if(_user is null)return;try{var users=await _repo.SearchUsersAsync(UserQuery,_user.Id);var following=await _repo.GetFollowingIdsAsync(_user.Id);People.Clear();foreach(var u in users)People.Add(new PersonItem(u.Id,u.UserName,u.DisplayName,u.Bio,following.Contains(u.Id)));Notify(nameof(CommunityMemberCount));FilterMessageContacts();await RefreshConversationSummariesAsync();Status=Persian?$"{People.Count} کاربر پیدا شد":$"Found {People.Count} people";}catch(Exception ex){Status=ex.Message;}}
 private async Task ToggleFollowAsync(object? parameter){if(_user is null||parameter is not PersonItem person)return;try{person.IsFollowing=await _repo.ToggleFollowAsync(_user.Id,person.Id);if(_followingOnly)await RefreshAsync();Status=Persian?"وضعیت دنبال‌کردن تغییر کرد":"Follow status updated";}catch(Exception ex){Status=ex.Message;}}
 public ICommand ToggleLikeCommand {get;}
 public ICommand AddCommentCommand {get;}
 private async Task ToggleLikeAsync(object? parameter){if(_user is null||parameter is not PostItem item)return;try{await _repo.ToggleLikeAsync(item.Id,_user.Id);await RefreshAsync();}catch(Exception ex){Status=ex.Message;}}
 private async Task AddCommentAsync(object? parameter){
  if(_user is null||parameter is not PostItem item)return;
  var content=item.CommentDraft.Trim();
  if(content.Length==0||content.Length>500){Status=Persian?"نظر باید بین ۱ تا ۵۰۰ نویسه باشد":"Comment must be between 1 and 500 characters";return;}
  try{
   await _repo.AddCommentAsync(item.Id,_user.Id,content);
   item.CommentDraft="";
   await RefreshAsync();
   Status=Persian?"نظر ثبت شد":"Comment added";
  }catch(Exception ex){Status=ex.Message;}
 }
 public ICommand SaveProfileCommand {get;}
 private async Task SaveProfileAsync(){try { if(_user is null) return; if(string.IsNullOrWhiteSpace(ProfileName)||ProfileName.Trim().Length>80){Status=Persian?"نام باید بین ۱ تا ۸۰ کاراکتر باشد":"Name must be 1–80 characters";return;} if(ProfileBio.Length>500){Status=Persian?"بیوگرافی حداکثر ۵۰۰ کاراکتر":"Bio must be at most 500 characters";return;} _user=await _repo.UpdateLocalProfileAsync(_user.Id,ProfileName.Trim(),ProfileBio.Trim()); await RefreshAsync(); Status=Persian?"پروفایل ذخیره شد":"Profile saved";}catch(Exception ex){Status=ex.Message;}}
 public ICommand PublishCommand {get;} public ICommand NavigateCommand {get;} public ICommand ToggleLanguageCommand {get;}
 public MainViewModel(ISocialRepository repo) { _repo=repo; ChoosePostImageCommand=new RelayCommand(_=>ChoosePostImage()); ClearPostImageCommand=new RelayCommand(_=>ClearSelectedPhotos()); MarkReadCommand=new AsyncParameterCommand(MarkReadAsync);MarkAllReadCommand=new AsyncCommand(MarkAllReadAsync);SendMessageCommand=new AsyncCommand(SendMessageAsync);RefreshMessagesCommand=new AsyncCommand(LoadConversationAsync);RefreshContactListCommand=new AsyncCommand(RefreshContactListAsync);RefreshNotificationsCommand=new AsyncCommand(RefreshNotificationsAsync); SearchPeopleCommand=new AsyncCommand(SearchPeopleAsync);OpenPublicProfileCommand=new AsyncParameterCommand(OpenPublicProfileAsync);ToggleFollowCommand=new AsyncParameterCommand(ToggleFollowAsync);ToggleLikeCommand=new AsyncParameterCommand(ToggleLikeAsync); AddCommentCommand=new AsyncParameterCommand(AddCommentAsync); PublishCommand=new AsyncCommand(PublishAsync); NavigateCommand=new RelayCommand(x=>Page=x?.ToString()??"Feed"); ToggleLanguageCommand=new RelayCommand(_=>{Persian=!Persian;Notify(nameof(ThemeLabel));SavePreferences();}); ToggleThemeCommand=new RelayCommand(_=>ToggleTheme()); SaveProfileCommand=new AsyncCommand(SaveProfileAsync); ChangeAvatarCommand=new AsyncCommand(()=>ChooseImageAsync(false)); ChangeCoverCommand=new AsyncCommand(()=>ChooseImageAsync(true)); }
 public async Task LoadAsync(SocialUser signedInUser) { try {LoadPreferences(); _user=signedInUser;ProfileName=_user.DisplayName;ProfileBio=_user.Bio;AvatarPath=_user.AvatarPath;CoverPath=_user.CoverPath; await RefreshAsync();await RefreshMyPostsAsync();await SearchPeopleAsync();await RefreshNotificationsAsync();await RefreshConversationSummariesAsync(); } catch(Exception ex){Status=ex.Message;} }
 private sealed class Preferences { public bool Persian {get;set;} public bool Light {get;set;} }
 private void SavePreferences(){try{System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_preferencesFile)!);System.IO.File.WriteAllText(_preferencesFile,System.Text.Json.JsonSerializer.Serialize(new Preferences{Persian=_persian,Light=_light}));}catch(System.IO.IOException){}catch(UnauthorizedAccessException){}}
 private void LoadPreferences(){try{if(!System.IO.File.Exists(_preferencesFile))return;var prefs=System.Text.Json.JsonSerializer.Deserialize<Preferences>(System.IO.File.ReadAllText(_preferencesFile));if(prefs is null)return;Persian=prefs.Persian;if(prefs.Light!=_light)ToggleTheme();}catch(System.Text.Json.JsonException){}catch(System.IO.IOException){}catch(UnauthorizedAccessException){}}
 private async Task RefreshAsync(){var drafts=Posts.ToDictionary(p=>p.Id,p=>p.CommentDraft);var posts=_followingOnly&&_user is not null?await _repo.GetFollowingFeedAsync(_user.Id):await _repo.GetFeedAsync(); Posts.Clear();foreach(var p in posts){var comments=await _repo.GetCommentsAsync(p.Id);var item=ToPostItem(p);if(drafts.TryGetValue(p.Id,out var draft))item.CommentDraft=draft;foreach(var c in comments)item.Comments.Add(new CommentItem(c.User.DisplayName,c.Content));Posts.Add(item);}await RefreshMyPostsAsync();Notify(nameof(VisiblePostCount));}
 private void ClearSelectedPhotos(){
  foreach(var path in SelectedPostImages.ToArray()){
   try{if(System.IO.File.Exists(path))System.IO.File.Delete(path);}catch(System.IO.IOException){}catch(UnauthorizedAccessException){}
  }
  SelectedPostImages.Clear(); SelectedPostImage=""; Notify(nameof(PhotoLimitLabel));Notify(nameof(CanPublish));
 }
 private void ChoosePostImage(){
  var dialog=new Microsoft.Win32.OpenFileDialog{Title=Persian?"انتخاب تصاویر پست":"Select post photos",Filter="Image files|*.png;*.jpg;*.jpeg;*.webp",Multiselect=true};
  if(dialog.ShowDialog()!=true)return;
  if(SelectedPostImages.Count+dialog.FileNames.Length>4){Status=Persian?"حداکثر ۴ تصویر مجاز است":"You can attach up to 4 photos";return;}
  var added=new System.Collections.Generic.List<string>();
  try{
   var folder=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AshkanSocialSphere","post-images");
   System.IO.Directory.CreateDirectory(folder);
   foreach(var name in dialog.FileNames){
    var file=new System.IO.FileInfo(name);
    if(file.Length>5*1024*1024)throw new InvalidOperationException("Each image must be 5 MB or smaller.");
    using(var stream=System.IO.File.OpenRead(file.FullName)){
     var decoder=System.Windows.Media.Imaging.BitmapDecoder.Create(stream,System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
     if(decoder.Frames.Count==0)throw new InvalidOperationException("Invalid image.");
    }
    var dest=System.IO.Path.Combine(folder,$"{Guid.NewGuid():N}{System.IO.Path.GetExtension(file.Name).ToLowerInvariant()}");
    System.IO.File.Copy(file.FullName,dest);added.Add(dest);
   }
   foreach(var path in added)SelectedPostImages.Add(path);
   SelectedPostImage=SelectedPostImages.FirstOrDefault()??"";
   Notify(nameof(PhotoLimitLabel));Notify(nameof(CanPublish));
  }catch(Exception ex){foreach(var path in added){try{System.IO.File.Delete(path);}catch(System.IO.IOException){}catch(UnauthorizedAccessException){}}Status=ex.Message;}
 }
 private async Task PublishAsync(){try{if(_user is null){Status="Profile not loaded.";return;}if(!CanPublish||Draft.Length>2000){Status=Persian?"پست باید متن یا تصویر داشته باشد و متن حداکثر ۲۰۰۰ نویسه باشد":"Add text or a photo; text must not exceed 2,000 characters";return;}await _repo.AddPostAsync(_user.Id,Draft.Trim(),PostImageCodec.Encode(SelectedPostImages));Draft="";SelectedPostImages.Clear();SelectedPostImage="";Notify(nameof(PhotoLimitLabel));Notify(nameof(CanPublish));await RefreshAsync();Status=Persian?"پست منتشر شد":"Post published successfully";}catch(Exception ex){Status=ex.Message;}}
}
public sealed class PostItem(Guid id,string author,string content,string created,int likes,bool liked,string imagePath=""):INotifyPropertyChanged {
 public string ImagePath {get;}=PostImageCodec.Decode(imagePath).FirstOrDefault()??""; public IReadOnlyList<string> Images {get;}=PostImageCodec.Decode(imagePath); public bool HasImage=>Images.Count>0; public Guid Id {get;}=id; public string Author {get;}=author; public string Content {get;}=content; public string Created {get;}=created; public int LikeCount {get;}=likes; public bool IsLiked {get;}=liked;
 public ObservableCollection<CommentItem> Comments {get;}=new(); public int CommentCount=>Comments.Count; private string _commentDraft=""; public string CommentDraft {get=>_commentDraft;set{_commentDraft=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(CommentDraft)));}}
 public event PropertyChangedEventHandler? PropertyChanged;
}
public sealed record CommentItem(string Author,string Content);
public sealed class AsyncParameterCommand(Func<object?,Task> execute):ICommand {private bool _running;public event EventHandler? CanExecuteChanged;public bool CanExecute(object? p)=>!_running;public async void Execute(object? p){if(_running)return;_running=true;CanExecuteChanged?.Invoke(this,EventArgs.Empty);try{await execute(p);}finally{_running=false;CanExecuteChanged?.Invoke(this,EventArgs.Empty);}}}
public sealed class RelayCommand(Action<object?> execute):ICommand {public event EventHandler? CanExecuteChanged {add{}remove{}} public bool CanExecute(object? p)=>true; public void Execute(object? p)=>execute(p);}
public sealed class AsyncCommand(Func<Task> execute):ICommand {private bool _running;public event EventHandler? CanExecuteChanged;public bool CanExecute(object? p)=>!_running;public async void Execute(object? p){if(_running)return;_running=true;CanExecuteChanged?.Invoke(this,EventArgs.Empty);try{await execute();}finally{_running=false;CanExecuteChanged?.Invoke(this,EventArgs.Empty);}}}

public sealed class PersonItem : INotifyPropertyChanged {
 public Guid Id {get;} public string UserName {get;} public string DisplayName {get;} public string Bio {get;}
 public string LastActivityLabel=>LastActivityUtc==DateTime.MinValue?"":LastActivityUtc.ToLocalTime().ToString("g");
 private DateTime _lastActivityUtc=DateTime.MinValue;public DateTime LastActivityUtc{get=>_lastActivityUtc;set{_lastActivityUtc=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(LastActivityLabel)));}}
 private int _unreadMessages;public int UnreadMessages{get=>_unreadMessages;set{_unreadMessages=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(UnreadMessages)));PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(UnreadBadge)));}}
 public string UnreadBadge=>UnreadMessages==0?"":$"● {UnreadMessages}";
 private string _lastMessagePreview="";public string LastMessagePreview{get=>_lastMessagePreview;set{_lastMessagePreview=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(LastMessagePreview)));}}
 private bool _isFollowing;public bool IsFollowing {get=>_isFollowing;set{_isFollowing=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(IsFollowing)));}}
 public event PropertyChangedEventHandler? PropertyChanged;
 public PersonItem(Guid id,string userName,string displayName,string bio,bool isFollowing){Id=id;UserName=userName;DisplayName=displayName;Bio=bio;_isFollowing=isFollowing;}
}

public sealed record MessageItem(string Author,string Content,string Created,bool IsMine,string DeliveryStatus);
public sealed class NotificationItem : INotifyPropertyChanged {
 public Guid Id {get;} public string Kind {get;} public string Detail {get;} public string Created {get;} public bool IsRead {get;}
 private bool _persian;
 public string KindLabel => Kind.ToLowerInvariant() switch {
  "like" => _persian ? "پسندیدن پست" : "Post liked",
  "comment" => _persian ? "نظر جدید" : "New comment",
  "follow" => _persian ? "دنبال‌کننده جدید" : "New follower",
  "message" => _persian ? "پیام جدید" : "New message",
  _ => Kind
 };
 public string ReadLabel => IsRead ? (_persian ? "خوانده‌شده" : "Read") : (_persian ? "خوانده‌نشده" : "Unread");
 public bool CanMarkRead => !IsRead;
 public event PropertyChangedEventHandler? PropertyChanged;
 public NotificationItem(Guid id,string kind,string detail,string created,bool isRead,bool persian){Id=id;Kind=kind;Detail=detail;Created=created;IsRead=isRead;_persian=persian;}
 public void UpdateLanguage(bool persian){_persian=persian;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(KindLabel)));PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(ReadLabel)));}
}

// Backwards compatible with the single-image ImagePath stored by earlier releases.
internal static class PostImageCodec {
 public static string Encode(System.Collections.Generic.IEnumerable<string> paths){var items=paths.ToArray();return items.Length==0?"":System.Text.Json.JsonSerializer.Serialize(items);}
 public static System.Collections.Generic.IReadOnlyList<string> Decode(string? value){
  if(string.IsNullOrWhiteSpace(value))return Array.Empty<string>();
  if(!value.StartsWith("[",StringComparison.Ordinal))return new[]{value};
  try{return System.Text.Json.JsonSerializer.Deserialize<string[]>(value)?.Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray()??Array.Empty<string>();}
  catch(System.Text.Json.JsonException){return Array.Empty<string>();}
 }
}
