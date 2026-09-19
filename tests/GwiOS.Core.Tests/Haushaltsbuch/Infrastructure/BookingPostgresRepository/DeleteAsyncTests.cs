using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.BookingPostgresRepository;

/// <summary>
/// Covers <c>BookingPostgresRepository.DeleteAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IBookingRepository _bookingRepository = database.BookingRepository;
    private readonly TestBookings _testBookings = new(database.CategoryRepository, database.BookingRepository);

    [Fact]
    public async Task DeletesOnlyTheBookingWithTheGivenId()
    {
        Booking toDelete = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);
        Booking toKeep = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(toDelete, TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(toKeep, TestContext.Current.CancellationToken);

        await _bookingRepository.DeleteAsync(toDelete.Id, TestContext.Current.CancellationToken);

        Assert.Null(await _testBookings.FindStoredAsync(toDelete.Id));
        Assert.NotNull(await _testBookings.FindStoredAsync(toKeep.Id));
    }

    [Fact]
    public async Task Succeeds_WhenTheBookingIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _bookingRepository.DeleteAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testBookings.DisposeAsync();
}
