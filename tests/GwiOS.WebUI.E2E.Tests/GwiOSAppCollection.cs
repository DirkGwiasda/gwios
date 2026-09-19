namespace GwiOS.WebUI.E2E.Tests;

/// <summary>
/// Groups all end-to-end tests. They share one running GwiOS.WebUI and one Chromium instance, and run one after
/// another so tests do not interfere with each other's data in the shared test database.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GwiOSAppCollection : ICollectionFixture<GwiOSAppFixture>
{
    /// <summary>
    /// Name that test classes pass to <see cref="CollectionAttribute"/> to join this collection.
    /// </summary>
    public const string Name = "GwiOSApp";
}
