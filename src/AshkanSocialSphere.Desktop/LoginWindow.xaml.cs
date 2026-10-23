using System.Windows;
using System.Windows.Controls;
using AshkanSocialSphere.Application;
using AshkanSocialSphere.Domain;
namespace AshkanSocialSphere.Desktop;
public partial class LoginWindow : Window
{
 private readonly IAccountService _accounts;
 private bool _register;
 private bool _persian;
 private bool _submitting;
 public SocialUser? SignedInUser { get; private set; }
 public LoginWindow(IAccountService accounts) { InitializeComponent(); _accounts = accounts; UpdateMode(); Loaded += (_, _) => UsernameInput.Focus(); }
 private void Mode_Click(object sender, RoutedEventArgs e) { _register = !_register; Feedback.Text = ""; UpdateMode(); ValidateFields(); UsernameInput.Focus(); }
 private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
 {
  if (Heading is null) return;
  _persian = LanguageSelector.SelectedIndex == 1;
  UpdateMode();
 }
 private void UpdateMode()
 {
  FlowDirection = _persian ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
  NameInput.Visibility = _register ? Visibility.Visible : Visibility.Collapsed;
  NameLabel.Visibility = NameInput.Visibility;
  NameError.Visibility = Visibility.Collapsed;
  LanguageLabel.Text = _persian ? "زبان" : "Language";
  HeroHeading.Text = _persian ? "دنیای تو.\nجامعه تو." : "Your world.\nYour community.";
  HeroSubtitle.Text = _persian ? "لحظه‌هایت را به اشتراک بگذار و با دیگران در ارتباط باش." : "Share moments, discover people, and stay connected.";
  Heading.Text = _persian ? (_register ? "ایجاد حساب کاربری" : "خوش برگشتی") : (_register ? "Join SocialSphere" : "Welcome back");
  Subheading.Text = _persian ? (_register ? "برای شروع حساب بسازید." : "برای ادامه وارد حساب شوید.") : (_register ? "Create your account to get started." : "Sign in to continue your journey.");
  UsernameLabel.Text = _persian ? "نام کاربری" : "Username";
  NameLabel.Text = _persian ? "نام نمایشی" : "Display name";
  PasswordLabel.Text = _persian ? "رمز عبور" : "Password";
  SubmitButton.Content = _persian ? (_register ? "ایجاد حساب" : "ورود") : (_register ? "Create account" : "Sign in");
  ModeButton.Content = _persian ? (_register ? "حساب دارید؟ وارد شوید" : "ساخت حساب جدید") : (_register ? "Already registered? Sign in" : "Create a new account");
  ValidateFields();
  PrivacyNote.Text = _persian ? "حساب محلی • حفاظت از رمز عبور با PBKDF2" : "Local accounts • Passwords protected with PBKDF2";
 }
 private void Fields_Changed(object sender, TextChangedEventArgs e) => ValidateFields();
 private void Password_Changed(object sender, RoutedEventArgs e) => ValidateFields();
 private bool ValidateFields()
 {
  // WPF can raise TextChanged during InitializeComponent before all named controls exist.
  if (UsernameInput is null || PasswordInput is null || NameInput is null || SubmitButton is null || PasswordHint is null || UsernameError is null || NameError is null || PasswordError is null) return false;
  var username = UsernameInput.Text.Trim();
  var passwordLength = PasswordInput.Password.Length;
  var usernameValid = username.Length is >= 3 and <= 30;
  var passwordValid = passwordLength is >= 10 and <= 128;
  var nameValid = !_register || !string.IsNullOrWhiteSpace(NameInput.Text);
  var valid = usernameValid && passwordValid && nameValid;
  // Show field-specific errors only after the user has entered a value.
  UsernameError.Text = _persian ? "نام کاربری باید بین ۳ تا ۳۰ نویسه باشد." : "Username must contain 3–30 characters.";
  UsernameError.Visibility = username.Length > 0 && !usernameValid ? Visibility.Visible : Visibility.Collapsed;
  PasswordError.Text = _persian ? "رمز عبور باید بین ۱۰ تا ۱۲۸ نویسه باشد." : "Password must contain 10–128 characters.";
  PasswordError.Visibility = passwordLength > 0 && !passwordValid ? Visibility.Visible : Visibility.Collapsed;
  NameError.Text = _persian ? "نام نمایشی الزامی است." : "Display name is required.";
  NameError.Visibility = _register && NameInput.Text.Length > 0 && !nameValid ? Visibility.Visible : Visibility.Collapsed;
  SubmitButton.IsEnabled = !_submitting && valid;
  PasswordHint.Visibility = _register ? Visibility.Visible : Visibility.Collapsed;
  if (_register)
  {
   PasswordHint.Text = _persian
    ? (passwordLength < 10 ? "رمز عبور باید حداقل ۱۰ نویسه داشته باشد." : "طول رمز عبور مناسب است؛ از ترکیب حروف، عدد و نماد استفاده کنید.")
    : (passwordLength < 10 ? "Use at least 10 characters for your password." : "Password length is sufficient; consider letters, numbers and symbols.");
  }
  return valid;
 }
 private async void Submit_Click(object sender, RoutedEventArgs e)
 {
  if (_submitting || !ValidateFields()) return;
  _submitting = true;
  SubmitButton.IsEnabled = false;
  ModeButton.IsEnabled = false;
  UsernameInput.IsEnabled = false;
  NameInput.IsEnabled = false;
  PasswordInput.IsEnabled = false;
  LanguageSelector.IsEnabled = false;
  Feedback.Text = _persian ? "در حال بررسی اطلاعات…" : "Checking your details…";
  try
  {
   var username = UsernameInput.Text.Trim();
   var password = PasswordInput.Password;
   var user = _register ? await _accounts.RegisterAsync(username, NameInput.Text.Trim(), password) : await _accounts.SignInAsync(username, password);
   if (user is null) { Feedback.Text = _persian ? "نام کاربری یا رمز عبور اشتباه است." : "Incorrect username or password."; return; }
   SignedInUser = user;
   DialogResult = true;
  }
  catch (ArgumentException)
  {
   Feedback.Text = _persian ? "اطلاعات واردشده معتبر نیست. نام کاربری ۳ تا ۳۰ نویسه و رمز عبور ۱۰ تا ۱۲۸ نویسه باشد." : "Invalid details. Username must be 3–30 characters and password 10–128 characters.";
  }
  catch (InvalidOperationException ex) when (ex.Message.Contains("Username already exists", StringComparison.Ordinal))
  {
   Feedback.Text = _persian ? "این نام کاربری قبلاً ثبت شده است." : "Username already exists.";
  }
  catch (Microsoft.EntityFrameworkCore.DbUpdateException)
  {
   Feedback.Text = _persian ? "ثبت اطلاعات انجام نشد. دوباره تلاش کنید." : "Could not save your account. Please try again.";
  }
  catch (Exception)
  {
   Feedback.Text = _persian ? "خطای غیرمنتظره رخ داد. دوباره تلاش کنید." : "An unexpected error occurred. Please try again.";
  }
  finally
  {
   _submitting = false;
   ModeButton.IsEnabled = true;
   UsernameInput.IsEnabled = true;
   NameInput.IsEnabled = true;
   PasswordInput.IsEnabled = true;
   LanguageSelector.IsEnabled = true;
   ValidateFields();
  }
 }
}
