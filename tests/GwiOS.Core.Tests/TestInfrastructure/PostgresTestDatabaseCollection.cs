namespace GwiOS.Core.Tests.TestInfrastructure;

/// <summary>
/// Groups all tests against the PostgreSQL test database. They share one <see cref="PostgresTestDatabase"/> and run
/// one after another, so the storage is created only once and unit tests never depend on the database.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestDatabase>
{
    /// <summary>
    /// Name that test classes pass to <see cref="CollectionAttribute"/> to join this collection.
    /// </summary>
    public const string Name = "PostgresTestDatabase";
}
