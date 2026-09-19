using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using RecurringBookingGenerationWorkflowUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.RecurringBookingGenerationWorkflow;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.RecurringBookingGenerationWorkflow;

/// <summary>
/// Runs a <c>RecurringBookingGenerationWorkflow</c> against fake managers and a recording logger, and gives the
/// tests access to all four.
/// </summary>
internal sealed class RecurringBookingGenerationWorkflowProbe
{
    public RecurringBookingGenerationWorkflowProbe()
    {
        Workflow = new RecurringBookingGenerationWorkflowUnderTest(
            RecurringItemManager,
            BookingManager,
            MonthReviewManager,
            Logger);
    }

    /// <summary>
    /// The fake recurring item manager the workflow reads active recurring items from.
    /// </summary>
    public RecurringItemManagerFake RecurringItemManager { get; } = new();

    /// <summary>
    /// The fake booking manager the workflow reads existing bookings from and creates confirmed bookings in.
    /// </summary>
    public BookingManagerFake BookingManager { get; } = new();

    /// <summary>
    /// The fake month review manager the workflow reads and marks review state in.
    /// </summary>
    public MonthReviewManagerFake MonthReviewManager { get; } = new();

    /// <summary>
    /// The logger the workflow writes to.
    /// </summary>
    public LoggerFake<RecurringBookingGenerationWorkflowUnderTest> Logger { get; } = new();

    /// <summary>
    /// The workflow under test.
    /// </summary>
    public RecurringBookingGenerationWorkflowUnderTest Workflow { get; }
}
