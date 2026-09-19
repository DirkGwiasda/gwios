using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.BookingPostgresRepository;

/// <summary>
/// Covers <c>BookingPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IBookingRepository _bookingRepository = database.BookingRepository;
    private readonly TestBookings _testBookings = new(database.CategoryRepository, database.BookingRepository);

    [Fact]
    public async Task KeepsExistingBookings_WhenTheTablesAlreadyExist()
    {
        Booking booking = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(booking, TestContext.Current.CancellationToken);

        await _bookingRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(await _testBookings.FindStoredAsync(booking.Id));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testBookings.DisposeAsync();
}
