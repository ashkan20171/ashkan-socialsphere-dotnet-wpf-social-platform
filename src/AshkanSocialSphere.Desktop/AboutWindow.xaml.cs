using System.Reflection;
using System.Windows;
namespace AshkanSocialSphere.Desktop;
public partial class AboutWindow : Window {
 public AboutWindow() {
  InitializeComponent();
  var version = Assembly.GetExecutingAssembly().GetName().Version;
  VersionText.Text = $"Version {version?.ToString(3) ?? "unknown"}";
 }
 private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
