using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using RecurringItemManagerUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

/// <summary>
/// Runs a <c>RecurringItemManager</c> against an in-memory repository, a configurable validator, a configurable
/// scheduler and a recording logger, and gives the tests access to all four.
/// </summary>
internal sealed class RecurringItemManagerProbe
{
    public RecurringItemManagerProbe()
    {
        RecurringItemManager = new RecurringItemManagerUnderTest(
            RecurringItemRepository,
            RecurringItemValidator,
            RecurringItemScheduler,
            Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its recurring items in.
    /// </summary>
    public RecurringItemRepositoryFake RecurringItemRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks recurring items with; every recurring item is valid until the test sets
    /// errors.
    /// </summary>
    public RecurringItemValidatorFake RecurringItemValidator { get; } = new();

    /// <summary>
    /// The scheduler the manager takes due dates from.
    /// </summary>
    public RecurringItemSchedulerFake RecurringItemScheduler { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<RecurringItemManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public RecurringItemManagerUnderTest RecurringItemManager { get; }
}
