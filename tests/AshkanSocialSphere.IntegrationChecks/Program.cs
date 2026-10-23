using AshkanSocialSphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

var database = Path.Combine(Path.GetTempPath(), $"ashkansocialsphere-check-{Guid.NewGuid():N}.db");
var options = new DbContextOptionsBuilder<SocialDbContext>().UseSqlite($"Data Source={database}").Options;
var passed = 0;
var failed = 0;
async Task Check(string name, Func<Task> action)
{
    try { await action(); Console.WriteLine($"PASS {name}"); passed++; }
    catch (Exception ex) { Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); failed++; }
}
void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
try
{
    await using var db = new SocialDbContext(options);
    await db.Database.EnsureCreatedAsync();
    var accounts = new AccountService(db);
    var social = new SqliteSocialRepository(db);
    var alice = await accounts.RegisterAsync("alice_test", "Alice", "CorrectHorseBattery123!");
    var bob = await accounts.RegisterAsync("bob_test", "Bob", "AnotherStrongPassword123!");
    await Check("Sign in succeeds with valid password", async () => Require((await accounts.SignInAsync("ALICE_TEST", "CorrectHorseBattery123!"))?.Id == alice.Id, "Expected Alice"));
    await Check("Sign in rejects incorrect password", async () => Require(await accounts.SignInAsync("alice_test", "wrong-password") is null, "Invalid password accepted"));
    await Check("Unknown account cannot sign in", async () => Require(await accounts.SignInAsync("not_a_real_user", "AnyPassword12345!") is null, "Unknown account authenticated"));
    await Check("Empty password cannot sign in", async () => Require(await accounts.SignInAsync("alice_test", "") is null, "Empty password authenticated"));
    await Check("Malformed stored password hash is rejected", async () => {
        var malformed = await accounts.RegisterAsync("malformed_test", "Malformed", "ValidPassword12345!");
        malformed.PasswordHash = "pbkdf2-sha256:210000:broken:broken";
        await db.SaveChangesAsync();
        Require(await accounts.SignInAsync("malformed_test", "ValidPassword12345!") is null, "Malformed hash accepted");
    });
    await Check("Unreasonably high PBKDF2 iteration count is rejected", async () => {
        var malformed = await db.Users.SingleAsync(x => x.UserName == "malformed_test");
        malformed.PasswordHash = "pbkdf2-sha256:999999999:AAAAAAAAAAAAAAAAAAAAAA==:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        await db.SaveChangesAsync();
        Require(await accounts.SignInAsync("malformed_test", "ValidPassword12345!") is null, "Excessive work factor accepted");
    });
    await Check("Duplicate username is rejected", async () => {
        try { await accounts.RegisterAsync("alice_test", "Duplicate", "YetAnotherStrongPassword!"); throw new Exception("Duplicate was accepted"); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists")) { }
    });
    await Check("Username format validation rejects invalid characters", async () => {
        try { await accounts.RegisterAsync("bad name", "Invalid", "ValidPassword12345!"); throw new Exception("Invalid username accepted"); }
        catch (ArgumentException) { }
    });
    await Check("Password policy rejects short passwords", async () => {
        try { await accounts.RegisterAsync("short_pass", "Invalid", "123"); throw new Exception("Short password accepted"); }
        catch (ArgumentException) { }
    });
    await Check("Empty post is rejected", async () => {
        try { await social.AddPostAsync(alice.Id, "   "); throw new Exception("Empty post accepted"); }
        catch (ArgumentException) { }
    });
    await Check("Post supports image without text", async () => {
        var post = await social.AddPostAsync(alice.Id, "", "sample-photo.jpg");
        Require(post.ImagePath == "sample-photo.jpg", "Image path lost");
    });
    var mainPost = await social.AddPostAsync(alice.Id, "Hello from integration checks");
    await Check("Comment is persisted", async () => {
        await social.AddCommentAsync(mainPost.Id, bob.Id, "Nice post!");
        var comments = await social.GetCommentsAsync(mainPost.Id);
        Require(comments.Any(c => c.UserId == bob.Id && c.Content == "Nice post!"), "Comment not found");
    });
    await Check("Like toggle adds and removes like", async () => {
        Require(await social.ToggleLikeAsync(mainPost.Id, bob.Id), "Like was not added");
        Require(!await social.ToggleLikeAsync(mainPost.Id, bob.Id), "Like was not removed");
    });
    await Check("Follow makes posts visible in following feed", async () => {
        Require(await social.ToggleFollowAsync(bob.Id, alice.Id), "Follow was not added");
        var feed = await social.GetFollowingFeedAsync(bob.Id);
        Require(feed.Any(p => p.Id == mainPost.Id), "Followed user's post missing");
    });
    await Check("Notifications can be marked read", async () => {
        var notifications = await social.GetNotificationsAsync(alice.Id);
        Require(notifications.Count > 0, "Expected notifications");
        await social.MarkNotificationReadAsync(notifications[0].Id, alice.Id);
        Require((await social.GetNotificationsAsync(alice.Id)).Any(n => n.Id == notifications[0].Id && n.IsRead), "Read flag not saved");
    });
    await Check("Empty message is rejected", async () => {
        try { await social.SendMessageAsync(alice.Id, bob.Id, "   "); throw new Exception("Empty message accepted"); }
        catch (ArgumentException) { }
    });
    await Check("Oversized message is rejected", async () => {
        try { await social.SendMessageAsync(alice.Id, bob.Id, new string('x', 2001)); throw new Exception("Oversized message accepted"); }
        catch (ArgumentException) { }
    });
    await Check("Self messaging is rejected", async () => {
        try { await social.SendMessageAsync(alice.Id, alice.Id, "hello"); throw new Exception("Self messaging accepted"); }
        catch (InvalidOperationException) { }
    });
    await Check("Unknown sender is rejected", async () => {
        try { await social.SendMessageAsync(Guid.NewGuid(), bob.Id, "hello"); throw new Exception("Unknown sender accepted"); }
        catch (InvalidOperationException) { }
    });
    await Check("Unknown recipient is rejected", async () => {
        try { await social.SendMessageAsync(alice.Id, Guid.NewGuid(), "hello"); throw new Exception("Unknown recipient accepted"); }
        catch (InvalidOperationException) { }
    });
    await Check("Notification cannot be marked read by another user", async () => {
        var target = (await social.GetNotificationsAsync(alice.Id)).FirstOrDefault(n => !n.IsRead);
        Require(target is not null, "Expected an unread notification");
        await social.MarkNotificationReadAsync(target!.Id, bob.Id);
        Require((await social.GetNotificationsAsync(alice.Id)).Any(n => n.Id == target.Id && !n.IsRead), "Unauthorized change accepted");
    });
    await Check("Unread count survives reopening the SQLite database", async () => {
        await social.SendMessageAsync(alice.Id, bob.Id, "Persisted unread check");
        await using var reopened = new SocialDbContext(options);
        var other = new SqliteSocialRepository(reopened);
        var summaries = await other.GetConversationSummariesAsync(bob.Id);
        Require(summaries.Any(x => x.ContactId == alice.Id && x.UnreadCount >= 1), "Unread message missing after reopen");
    });
    await Check("Mark read only affects selected incoming conversation", async () => {
        await social.MarkConversationReadAsync(bob.Id, alice.Id);
        var summaries = await social.GetConversationSummariesAsync(bob.Id);
        Require(summaries.First(x => x.ContactId == alice.Id).UnreadCount == 0, "Read count did not clear");
        await social.SendMessageAsync(bob.Id, alice.Id, "Bob reply");
        await social.MarkConversationReadAsync(bob.Id, alice.Id);
        var aliceSummaries = await social.GetConversationSummariesAsync(alice.Id);
        Require(aliceSummaries.First(x => x.ContactId == bob.Id).UnreadCount == 1, "Another user's incoming message was marked read");
    });
    await Check("Read receipts are persisted for recipient only", async () => {
        await social.SendMessageAsync(alice.Id, bob.Id, "Receipt validation");
        await social.MarkConversationReadAsync(bob.Id, alice.Id);
        await using var reopened = new SocialDbContext(options);
        var message = await reopened.DirectMessages.AsNoTracking()
            .Where(m => m.SenderId == alice.Id && m.RecipientId == bob.Id && m.Content == "Receipt validation")
            .SingleAsync();
        Require(message.ReadUtc is not null, "Recipient read timestamp not persisted");
        await social.SendMessageAsync(bob.Id, alice.Id, "Unseen by Alice");
        await social.MarkConversationReadAsync(bob.Id, alice.Id);
        await using var verify = new SocialDbContext(options);
        var reverse = await verify.DirectMessages.AsNoTracking()
            .Where(m => m.SenderId == bob.Id && m.RecipientId == alice.Id && m.Content == "Unseen by Alice")
            .SingleAsync();
        Require(reverse.ReadUtc is null, "Sender marked a message read for another recipient");
    });
    await Check("Conversation returns at most 200 newest messages in chronological order", async () => {
        for (var i = 0; i < 205; i++)
            await social.SendMessageAsync(alice.Id, bob.Id, $"Paging test {i:D3}");
        var messages = await social.GetConversationAsync(alice.Id, bob.Id);
        Require(messages.Count == 200, "Conversation page size is not capped at 200");
        Require(messages.Any(m => m.Content == "Paging test 204"), "Newest message is missing");
        Require(!messages.Any(m => m.Content == "Paging test 000"), "Oldest message was not paged out");
        Require(messages.Zip(messages.Skip(1)).All(pair => pair.First.CreatedUtc <= pair.Second.CreatedUtc),
            "Messages are not chronologically sorted");
    });
    await Check("SQLite integrity check passes", async () => {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={database}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        var result = (string?)await command.ExecuteScalarAsync();
        Require(result == "ok", $"SQLite integrity check failed: {result}");
    });
    await Check("Message appears in conversation", async () => {
        await social.SendMessageAsync(alice.Id, bob.Id, "Private hello");
        var messages = await social.GetConversationAsync(bob.Id, alice.Id);
        Require(messages.Any(m => m.Content == "Private hello"), "Message not found");
    });
}
finally
{
    // Only delete this isolated test database; never touch the real user's profile database.
    foreach (var suffix in new[] { "", "-wal", "-shm" })
    {
        var path = database + suffix;
        if (File.Exists(path)) File.Delete(path);
    }
}
Console.WriteLine($"Integration checks: {passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;
