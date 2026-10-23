using System.Security.Cryptography;
using AshkanSocialSphere.Application;
using AshkanSocialSphere.Domain;
using Microsoft.EntityFrameworkCore;
namespace AshkanSocialSphere.Infrastructure;
public sealed class AccountService(SocialDbContext db) : IAccountService {
 private const int Iterations=210000;
 public async Task<SocialUser> RegisterAsync(string username,string displayName,string password,CancellationToken ct=default) {
  username=(username??"").Trim().ToLowerInvariant();displayName=(displayName??"").Trim();
  ArgumentNullException.ThrowIfNull(password);
  if(username.Length<3||username.Length>30||!username.All(c=>char.IsAsciiLetterLower(c)||char.IsAsciiDigit(c)||c=='_'))throw new ArgumentException("Username: 3–30 letters, digits or underscores.");
  if(displayName.Length<1||displayName.Length>80)throw new ArgumentException("Display name must be 1–80 characters.");
  if(password.Length<10||password.Length>128)throw new ArgumentException("Password must be 10–128 characters.");
  if(await db.Users.AnyAsync(x=>x.UserName==username,ct))throw new InvalidOperationException("Username already exists.");
  byte[] salt=RandomNumberGenerator.GetBytes(16);byte[] hash=Rfc2898DeriveBytes.Pbkdf2(password,salt,Iterations,HashAlgorithmName.SHA256,32);
  var user=new SocialUser{UserName=username,DisplayName=displayName,PasswordHash=$"pbkdf2-sha256:{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}"};
  db.Users.Add(user);await db.SaveChangesAsync(ct);return user;
 }
 public async Task<SocialUser?> SignInAsync(string username,string password,CancellationToken ct=default) {
  username=(username??"").Trim().ToLowerInvariant();if(string.IsNullOrEmpty(password))return null;var user=await db.Users.AsNoTracking().FirstOrDefaultAsync(x=>x.UserName==username,ct);
  if(user is null||string.IsNullOrWhiteSpace(user.PasswordHash))return null;
  var parts=user.PasswordHash.Split(':');if(parts.Length!=4||parts[0]!="pbkdf2-sha256"||!int.TryParse(parts[1],out int iterations)||iterations<100000||iterations>1000000)return null;
  try {var salt=Convert.FromBase64String(parts[2]);var expected=Convert.FromBase64String(parts[3]);if(salt.Length!=16||expected.Length!=32)return null;var actual=Rfc2898DeriveBytes.Pbkdf2(password,salt,iterations,HashAlgorithmName.SHA256,32);return CryptographicOperations.FixedTimeEquals(actual,expected)?user:null;}catch(FormatException){return null;}
 }
}
