namespace Atlas.Persistence.Tests;

/// <summary>Owns only this test process's randomly named databases.</summary>
[TestClass]
public sealed class SqlTestDatabaseLifecycle
{
    [AssemblyCleanup]
    public static void Cleanup() => SqlTestDatabase.DeletePooledDatabases();
}
