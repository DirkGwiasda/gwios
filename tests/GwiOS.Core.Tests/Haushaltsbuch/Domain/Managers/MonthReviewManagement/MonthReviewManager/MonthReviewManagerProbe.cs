using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using MonthReviewManagerUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.MonthReviewManager;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.MonthReviewManager;

/// <summary>
/// Runs a <c>MonthReviewManager</c> against an in-memory repository, a configurable validator, a clock that stands
/// still and a recording logger, and gives the tests access to all four.
/// </summary>
internal sealed class MonthReviewManagerProbe
{
    public MonthReviewManagerProbe()
    {
        MonthReviewManager = new MonthReviewManagerUnderTest(
            MonthReviewRepository,
            MonthReviewValidator,
            TimeProvider,
            Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its month reviews in.
    /// </summary>
    public MonthReviewRepositoryFake MonthReviewRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks month reviews with; every month review is valid until the test sets errors.
    /// </summary>
    public MonthReviewValidatorFake MonthReviewValidator { get; } = new();

    /// <summary>
    /// The clock the manager reads the current time from.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<MonthReviewManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public MonthReviewManagerUnderTest MonthReviewManager { get; }
}
