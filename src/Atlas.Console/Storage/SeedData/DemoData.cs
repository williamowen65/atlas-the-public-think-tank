using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Opt-in demo records with fixed identities and timestamps.</summary>
internal static partial class DemoData
{
    public static void Seed(AtlasDataContext database)
    {
        SeedNode(database);
        SeedNodeType(database);
        SeedDocument(database);
        SeedBlock(database);
        SeedVote(database);
        SeedReactionDefinition(database);
        SeedNodeReaction(database);
        SeedCommunity(database);
        SeedCommunityMembership(database);
        SeedCommunityNode(database);
        SeedComment(database);
        SeedModerationCase(database);
    }
}
