using System.Windows;
using AshkanSocialSphere.Desktop.ViewModels;
using AshkanSocialSphere.Domain;
namespace AshkanSocialSphere.Desktop;
public partial class MainWindow : Window {
 private readonly MainViewModel _vm;
 private readonly System.Windows.Threading.DispatcherTimer _chatRefreshTimer = new() { Interval = TimeSpan.FromSeconds(15) };
 private bool _refreshInProgress;
 private bool _wasNearChatBottom = true;
 public MainWindow(MainViewModel vm, SocialUser user) {
  InitializeComponent(); _vm=vm; DataContext=vm;
  UpdateResponsiveLayout(ActualWidth > 0 ? ActualWidth : Width);
  // Capture scroll position BEFORE a collection change causes the layout to recalculate.
  ChatHistoryScrollViewer.ScrollChanged += (_, args) => {
   if (args.ExtentHeightChange == 0 && args.ViewportHeightChange == 0)
    _wasNearChatBottom = args.ExtentHeight - args.ViewportHeight - args.VerticalOffset <= 90;
  };
  _vm.VisibleConversation.CollectionChanged += (_, _) => {
   var shouldFollow = _wasNearChatBottom;
   Dispatcher.BeginInvoke(new Action(() => {
    if (!IsLoaded || !IsVisible || !_vm.IsMessages || !shouldFollow) return;
    ChatHistoryScrollViewer.ScrollToEnd();
   }), System.Windows.Threading.DispatcherPriority.Loaded);
  };
  _chatRefreshTimer.Tick += ChatRefreshTimer_Tick;
  Loaded += async (_,_) => { await _vm.LoadAsync(user); _chatRefreshTimer.Start(); };
  Closed += (_,_) => _chatRefreshTimer.Stop();
 }
 private async void ChatRefreshTimer_Tick(object? sender, EventArgs e) {
  if (_refreshInProgress || !_vm.IsMessages || !IsVisible) return;
  _refreshInProgress = true;
  try { await _vm.RefreshChatInBackgroundAsync(); }
  catch (Exception) { /* The next tick retries; the view model reports repository failures. */ }
  finally { _refreshInProgress = false; }
 }
 // Adapt the shell without removing access to navigation or affecting stored data.
 private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateResponsiveLayout(e.NewSize.Width);
 private void UpdateResponsiveLayout(double width) {
  if (NavigationColumn is null || CommunityColumn is null || CommunitySidebar is null) return;
  bool compact = width < 980;
  bool hideCommunity = width < 1280;
  NavigationColumn.Width = new GridLength(compact ? 184 : 255);
  CommunityColumn.Width = hideCommunity ? new GridLength(0) : new GridLength(285);
  CommunitySidebar.Visibility = hideCommunity ? Visibility.Collapsed : Visibility.Visible;
  if (MainContentScroll is not null)
   MainContentScroll.Padding = compact ? new Thickness(14, 18, 14, 18) : new Thickness(22, 24, 22, 24);
 }
 private async void DeleteOwnPost_Click(object sender,RoutedEventArgs e) {
  if(sender is not System.Windows.Controls.Button { Tag: PostItem post })return;
  var question=_vm.Persian?"این پست برای همیشه حذف شود؟":"Delete this post permanently?";
  var answer=MessageBox.Show(this,question,_vm.Persian?"تأیید حذف":"Confirm deletion",MessageBoxButton.YesNo,MessageBoxImage.Warning);
  if(answer==MessageBoxResult.Yes)await _vm.DeleteOwnPostAsync(post.Id);
 }
 private async void EditOwnPost_Click(object sender,RoutedEventArgs e) {
  if(sender is not System.Windows.Controls.Button { Tag: PostItem post })return;
  var fa=_vm.Persian;
  var dialog=new Window { Title=fa?"ویرایش پست و تصاویر":"Edit post and photos",Owner=this,Width=650,Height=590,WindowStartupLocation=WindowStartupLocation.CenterOwner,MinWidth=430,MinHeight=390,FlowDirection=_vm.LayoutDirection };
  var layout=new System.Windows.Controls.DockPanel { Margin=new Thickness(20) };
  var actions=new System.Windows.Controls.StackPanel { Orientation=System.Windows.Controls.Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };
  var save=new System.Windows.Controls.Button { Content=fa?"ذخیره تغییرات":"Save changes",Padding=new Thickness(18,9,18,9),Margin=new Thickness(6) };
  var cancel=new System.Windows.Controls.Button { Content=fa?"انصراف":"Cancel",Padding=new Thickness(18,9,18,9),Margin=new Thickness(6),IsCancel=true };
  actions.Children.Add(cancel);actions.Children.Add(save);
  System.Windows.Controls.DockPanel.SetDock(actions,System.Windows.Controls.Dock.Bottom);layout.Children.Add(actions);
  var panel=new System.Windows.Controls.StackPanel();
  var editor=new System.Windows.Controls.TextBox { Text=post.Content,TextWrapping=TextWrapping.Wrap,AcceptsReturn=true,VerticalScrollBarVisibility=System.Windows.Controls.ScrollBarVisibility.Auto,MaxLength=2000,Height=125,FlowDirection=_vm.LayoutDirection };
  panel.Children.Add(editor);
  var count=new System.Windows.Controls.TextBlock { Margin=new Thickness(0,6,0,12),Foreground=System.Windows.Media.Brushes.Gray };
  void UpdateCount()=>count.Text=$"{editor.Text.Length}/2000";
  editor.TextChanged+=(_,_)=>UpdateCount();UpdateCount();panel.Children.Add(count);
  var photos=post.Images.ToList();
  var photoCount=new System.Windows.Controls.TextBlock { Margin=new Thickness(0,0,0,8),Foreground=System.Windows.Media.Brushes.Gray };
  var thumbnails=new System.Windows.Controls.WrapPanel();
  void RefreshPhotos(){
   thumbnails.Children.Clear();photoCount.Text=fa?$"{photos.Count} تصویر از ۴":$"{photos.Count} of 4 photos";
   foreach(var path in photos.ToArray()){
    var item=new System.Windows.Controls.StackPanel { Margin=new Thickness(0,0,12,12) };
    if(System.IO.File.Exists(path))item.Children.Add(new System.Windows.Controls.Image { Source=new System.Windows.Media.Imaging.BitmapImage(new Uri(path,UriKind.Absolute)),Width=112,Height=90,Stretch=System.Windows.Media.Stretch.UniformToFill });
    var remove=new System.Windows.Controls.Button { Content=fa?"حذف از پست":"Remove",Tag=path,Margin=new Thickness(0,4,0,0) };
    remove.Click+=(_,_)=>{photos.Remove(path);RefreshPhotos();};item.Children.Add(remove);thumbnails.Children.Add(item);
   }
  }
  panel.Children.Add(photoCount);
  var add=new System.Windows.Controls.Button { Content=fa?"افزودن تصویر":"Add photo",HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,12),Padding=new Thickness(12,7,12,7) };
  add.Click+=(_,_)=>{
   var picker=new Microsoft.Win32.OpenFileDialog { Filter="Images|*.png;*.jpg;*.jpeg;*.webp;*.bmp",Multiselect=true };
   if(picker.ShowDialog(dialog)!=true)return;
   var candidates=picker.FileNames.Where(x=>System.IO.File.Exists(x)&&!photos.Contains(x,StringComparer.OrdinalIgnoreCase)).ToArray();
   if(photos.Count+candidates.Length>4){MessageBox.Show(dialog,fa?"حداکثر چهار تصویر مجاز است":"Up to four images are allowed");return;}
   photos.AddRange(candidates);RefreshPhotos();
  };
  panel.Children.Add(add);panel.Children.Add(thumbnails);RefreshPhotos();
  layout.Children.Add(new System.Windows.Controls.ScrollViewer { Content=panel,VerticalScrollBarVisibility=System.Windows.Controls.ScrollBarVisibility.Auto });
  dialog.Content=layout;
  save.Click+=(_,_)=>{
   if(string.IsNullOrWhiteSpace(editor.Text)&&photos.Count==0){MessageBox.Show(dialog,fa?"پست باید متن یا تصویر داشته باشد":"A post needs text or a photo");return;}
   dialog.DialogResult=true;
  };
  cancel.Click+=(_,_)=>dialog.DialogResult=false;
  if(dialog.ShowDialog()==true)await _vm.EditOwnPostWithImagesAsync(post.Id,editor.Text,photos);
 }
 private string DraftPath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AshkanSocialSphere","post-draft.txt");
 private void SaveDraft_Click(object sender,RoutedEventArgs e){
  try{System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DraftPath)!);System.IO.File.WriteAllText(DraftPath,_vm.Draft);_vm.Status=_vm.Persian?"پیش‌نویس روی این دستگاه ذخیره شد":"Draft saved on this device";}
  catch(Exception){_vm.Status=_vm.Persian?"ذخیره پیش‌نویس ناموفق بود":"Unable to save draft";}
 }
 private void LoadDraft_Click(object sender,RoutedEventArgs e){
  try{if(System.IO.File.Exists(DraftPath)){_vm.Draft=System.IO.File.ReadAllText(DraftPath);_vm.Status=_vm.Persian?"پیش‌نویس بازیابی شد":"Draft restored";}else _vm.Status=_vm.Persian?"پیش‌نویسی یافت نشد":"No saved draft found";}
  catch(Exception){_vm.Status=_vm.Persian?"بازیابی پیش‌نویس ناموفق بود":"Unable to restore draft";}
 }
 private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();
 private void Logout_Click(object sender,RoutedEventArgs e)=>Close();

 private void PreviewPostImage_Click(object sender, RoutedEventArgs e) {
  if(sender is not System.Windows.Controls.Button button || button.Tag is not string path || !System.IO.File.Exists(path)) return;
  try {
   var image = new System.Windows.Controls.Image { Stretch = System.Windows.Media.Stretch.Uniform, Margin = new Thickness(16) };
   var bitmap = new System.Windows.Media.Imaging.BitmapImage();
   bitmap.BeginInit();
   bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
   bitmap.UriSource = new Uri(path, UriKind.Absolute);
   bitmap.EndInit(); bitmap.Freeze(); image.Source = bitmap;
   var preview = new Window { Title = "SocialSphere · Image preview", Owner = this,
    WindowStartupLocation = WindowStartupLocation.CenterOwner, Width = 900, Height = 680,
    MinWidth = 360, MinHeight = 260, Background = System.Windows.Media.Brushes.Black,
    Content = new System.Windows.Controls.ScrollViewer { Content = image, HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto } };
   preview.PreviewKeyDown += (_, args) => { if (args.Key == System.Windows.Input.Key.Escape) preview.Close(); };
   preview.ShowDialog();
  } catch(Exception ex) { MessageBox.Show(this, ex.Message, "Image preview", MessageBoxButton.OK, MessageBoxImage.Warning); }
 }
}
