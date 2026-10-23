using System.IO;
using System.Windows;
using AshkanSocialSphere.Application;
using AshkanSocialSphere.Infrastructure;
using AshkanSocialSphere.Desktop.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace AshkanSocialSphere.Desktop;
public partial class App : System.Windows.Application {
 private ServiceProvider? _services;
 protected override async void OnStartup(StartupEventArgs e){base.OnStartup(e);ShutdownMode=ShutdownMode.OnExplicitShutdown;
  try{var services=new ServiceCollection();var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AshkanSocialSphere");Directory.CreateDirectory(folder);
   services.AddDbContext<SocialDbContext>(o=>o.UseSqlite($"Data Source={Path.Combine(folder,"socialsphere.db")};Foreign Keys=True;Default Timeout=30;Pooling=True"));
   services.AddScoped<ISocialRepository,SqliteSocialRepository>();services.AddScoped<IAccountService,AccountService>();services.AddTransient<MainViewModel>();_services=services.BuildServiceProvider();
   using(var scope=_services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();await db.Database.EnsureCreatedAsync();
    // WAL allows concurrent readers while writes are serialized; busy timeout reduces transient lock failures.
    await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
    await db.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=30000;");
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS \"Comments\" (\"Id\" TEXT NOT NULL CONSTRAINT \"PK_PostComment\" PRIMARY KEY, \"PostId\" TEXT NOT NULL, \"UserId\" TEXT NOT NULL, \"Content\" TEXT NOT NULL, \"CreatedUtc\" TEXT NOT NULL, FOREIGN KEY(\"UserId\") REFERENCES \"Users\"(\"Id\") ON DELETE CASCADE)");}
   using(var scope=_services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS \"Follows\" (\"FollowerId\" TEXT NOT NULL, \"FollowingId\" TEXT NOT NULL, \"CreatedUtc\" TEXT NOT NULL, PRIMARY KEY (\"FollowerId\",\"FollowingId\"), FOREIGN KEY(\"FollowerId\") REFERENCES \"Users\"(\"Id\") ON DELETE RESTRICT, FOREIGN KEY(\"FollowingId\") REFERENCES \"Users\"(\"Id\") ON DELETE RESTRICT)");}
   using(var scope=_services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS \"DirectMessages\" (\"Id\" TEXT NOT NULL PRIMARY KEY, \"SenderId\" TEXT NOT NULL, \"RecipientId\" TEXT NOT NULL, \"Content\" TEXT NOT NULL, \"CreatedUtc\" TEXT NOT NULL)");
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS \"Notifications\" (\"Id\" TEXT NOT NULL PRIMARY KEY, \"UserId\" TEXT NOT NULL, \"ActorId\" TEXT NOT NULL, \"Kind\" TEXT NOT NULL, \"Detail\" TEXT NOT NULL, \"CreatedUtc\" TEXT NOT NULL)");}
   // Backward-compatible upgrade for existing Phase 09/10 databases.
   using(var scope=_services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    var connection=db.Database.GetDbConnection();await connection.OpenAsync();
    try{
     using var command=connection.CreateCommand();command.CommandText="PRAGMA table_info('Notifications')";
     bool hasRead=false;using(var reader=await command.ExecuteReaderAsync()){
      while(await reader.ReadAsync())if(string.Equals(reader.GetString(1),"IsRead",StringComparison.OrdinalIgnoreCase))hasRead=true;
     }
     if(!hasRead)await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"Notifications\" ADD COLUMN \"IsRead\" INTEGER NOT NULL DEFAULT 0");
    }finally{await connection.CloseAsync();}
   }
   // Additive upgrade: preserve existing profiles and user data.
   using(var scope=_services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    var conn=db.Database.GetDbConnection();await conn.OpenAsync();
    try{
     foreach(var column in new[]{"AvatarPath","CoverPath"}){
      bool exists=false;
      using(var cmd=conn.CreateCommand()){
       cmd.CommandText="PRAGMA table_info('Users')";
       using var reader=await cmd.ExecuteReaderAsync();
       while(await reader.ReadAsync()) if(string.Equals(reader.GetString(1),column,StringComparison.OrdinalIgnoreCase))exists=true;
      }
      if(!exists)await db.Database.ExecuteSqlRawAsync($"ALTER TABLE \"Users\" ADD COLUMN \"{column}\" TEXT NOT NULL DEFAULT ''");
     }
    }finally{await conn.CloseAsync();}
   }
   // Additive migration for image posts created with Phase 17.
   using(var scope=_services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    var conn=db.Database.GetDbConnection();await conn.OpenAsync();
    try{
     bool exists=false;
     using(var cmd=conn.CreateCommand()){
      cmd.CommandText="PRAGMA table_info('Posts')";
      using var reader=await cmd.ExecuteReaderAsync();
      while(await reader.ReadAsync()) if(string.Equals(reader.GetString(1),"ImagePath",StringComparison.OrdinalIgnoreCase))exists=true;
     }
     if(!exists)await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"Posts\" ADD COLUMN \"ImagePath\" TEXT NOT NULL DEFAULT ''");
    }finally{await conn.CloseAsync();}
   }
   // Phase 33: additive, idempotent migration; existing message history is preserved.
   using(var scope=_services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    var conn=db.Database.GetDbConnection();await conn.OpenAsync();
    try{
     bool exists=false;
     using(var cmd=conn.CreateCommand()){
      cmd.CommandText="PRAGMA table_info('DirectMessages')";
      using var reader=await cmd.ExecuteReaderAsync();
      while(await reader.ReadAsync())if(string.Equals(reader.GetString(1),"ReadUtc",StringComparison.OrdinalIgnoreCase))exists=true;
     }
     if(!exists)await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"DirectMessages\" ADD COLUMN \"ReadUtc\" TEXT NULL");
    }finally{await conn.CloseAsync();}
   }
   // Phase 38: idempotent indexes for recent chats and unread-message lookups.
   // Run after ReadUtc migration to support databases created by earlier phases.
   using(var scope=_services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"IX_DirectMessages_Sender_Recipient_Created\" ON \"DirectMessages\" (\"SenderId\", \"RecipientId\", \"CreatedUtc\")");
    await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"IX_DirectMessages_Recipient_Sender_Read\" ON \"DirectMessages\" (\"RecipientId\", \"SenderId\", \"ReadUtc\")");
   }
   var loginScope=_services.CreateScope();var login=new LoginWindow(loginScope.ServiceProvider.GetRequiredService<IAccountService>());var result=login.ShowDialog();
   if(result!=true||login.SignedInUser is null){loginScope.Dispose();Shutdown();return;}
   var vm=loginScope.ServiceProvider.GetRequiredService<MainViewModel>();var main=new MainWindow(vm,login.SignedInUser);MainWindow=main;ShutdownMode=ShutdownMode.OnMainWindowClose;main.Closed+=(_,_)=>loginScope.Dispose();main.Show();
  }catch(Exception ex){MessageBox.Show($"Unable to start: {ex.Message}","Startup error",MessageBoxButton.OK,MessageBoxImage.Error);Shutdown(1);}
 }
 protected override void OnExit(ExitEventArgs e){_services?.Dispose();base.OnExit(e);}
}
