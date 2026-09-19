using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.BookingPostgresRepository;

/// <summary>
/// Creates bookings, and the category a foreign key requires them to have, for the tests against the PostgreSQL
/// test database, and deletes everything it created when it is disposed (bookings before their category, as the
/// foreign key requires). The database may contain bookings of other tests, so tests only look at the bookings
/// they created themselves.
/// </summary>
public sealed class TestBookings : IAsyncDisposable
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly TestCategories _testCategories;
    private readonly List<Guid> _bookingIds = [];

    public TestBookings(ICategoryRepository categoryRepository, IBookingRepository bookingRepository)
    {
        _categoryRepository = categoryRepository;
        _bookingRepository = bookingRepository;
        _testCategories = new TestCategories(categoryRepository);
    }

    /// <summary>
    /// Creates and stores a new expense category, then returns a new, not yet stored, booked expense booking for
    /// it on the given date.
    /// </summary>
    public async Task<Booking> CreateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        Category category = _testCategories.Create();
        await _categoryRepository.InsertAsync(category, cancellationToken);
        Booking booking = new()
        {
            Date = date, Type = category.Type, CategoryId = category.Id, Amount = 58m,
            Status = BookingStatus.Booked
        };
        _bookingIds.Add(booking.Id);
        return booking;
    }

    /// <summary>
    /// Returns the stored booking with the given ID, or <c>null</c> if no such booking is stored.
    /// </summary>
    public async Task<Booking?> FindStoredAsync(Guid bookingId)
    {
        List<Booking> bookings = await _bookingRepository.GetInRangeAsync(DateOnly.MinValue, DateOnly.MaxValue);
        return bookings.SingleOrDefault(booking => booking.Id == bookingId);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (Guid bookingId in _bookingIds)
        {
            await _bookingRepository.DeleteAsync(bookingId);
        }

        await _testCategories.DisposeAsync();
    }
}
