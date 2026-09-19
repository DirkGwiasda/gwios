using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using BookingManagerUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.BookingManager;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.BookingManager;

/// <summary>
/// Runs a <c>BookingManager</c> against an in-memory repository, a configurable validator and a recording logger,
/// and gives the tests access to all three.
/// </summary>
internal sealed class BookingManagerProbe
{
    public BookingManagerProbe()
    {
        BookingManager = new BookingManagerUnderTest(BookingRepository, BookingValidator, Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its bookings in.
    /// </summary>
    public BookingRepositoryFake BookingRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks bookings with; every booking is valid until the test sets errors.
    /// </summary>
    public BookingValidatorFake BookingValidator { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<BookingManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public BookingManagerUnderTest BookingManager { get; }
}
