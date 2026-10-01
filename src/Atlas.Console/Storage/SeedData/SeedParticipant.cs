namespace Atlas.ConsoleApp.Storage;

internal sealed record DemoParticipant(string Email, string DisplayName);

/// <summary>
/// Demo identities are created through Atlas registration. Generated IDs are then reused by every
/// dependent seed record so authorship, votes, communities, and reactions point at real accounts.
/// </summary>
internal static class DemoParticipants
{
    public const string Password = "Atlas_Demo_Only_94!Password";

    public static readonly DemoParticipant[] Accounts =
    [
        new("demo01@example.test", "Demo User 01"),
        new("demo02@example.test", "Demo User 02"),
        new("demo03@example.test", "Demo User 03"),
        new("demo04@example.test", "Demo User 04"),
        new("demo05@example.test", "Demo User 05"),
    ];

    public static Guid User01Id { get; private set; }
    public static Guid User02Id { get; private set; }
    public static Guid User03Id { get; private set; }
    public static Guid User04Id { get; private set; }
    public static Guid User05Id { get; private set; }

    public static void SetIds(IReadOnlyList<Guid> ids)
    {
        if (ids.Count != Accounts.Length || ids.Any(id => id == Guid.Empty))
            throw new ArgumentException("All demo participant IDs are required.", nameof(ids));

        User01Id = ids[0];
        User02Id = ids[1];
        User03Id = ids[2];
        User04Id = ids[3];
        User05Id = ids[4];
    }
}
