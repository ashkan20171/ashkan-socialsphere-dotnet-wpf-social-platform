using AshkanSocialSphere.Domain;

var checks = new (string Name, Func<bool> Check)[] {
    ("New user has unique identifier", () => new SocialUser().Id != new SocialUser().Id),
    ("New post has unique identifier", () => new SocialPost().Id != new SocialPost().Id),
    ("New comment has unique identifier", () => new PostComment().Id != new PostComment().Id),
    ("New notifications start unread", () => !new SocialNotification().IsRead),
    ("New post has empty image path", () => new SocialPost().ImagePath == ""),
    ("New user has empty avatar and cover", () => new SocialUser().AvatarPath == "" && new SocialUser().CoverPath == ""),
    ("New post has empty likes collection", () => new SocialPost().Likes.Count == 0),
    ("UTC creation timestamps", () => new SocialPost().CreatedUtc.Kind == DateTimeKind.Utc),
};
var failed = 0;
foreach (var (name, check) in checks) {
    try { if (!check()) throw new Exception("Assertion returned false"); Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failed++; Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); }
}
Console.WriteLine($"{checks.Length - failed}/{checks.Length} checks passed");
return failed == 0 ? 0 : 1;
