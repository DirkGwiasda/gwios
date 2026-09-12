using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.Contracts;
using ToDoManagerUnderTest = GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.ToDoManager;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.ToDoManager;

/// <summary>
/// Runs a <c>ToDoManager</c> against an in-memory repository, a configurable validator, a clock that stands still and
/// a recording logger, and gives the tests access to all four.
/// </summary>
internal sealed class ToDoManagerProbe
{
    public ToDoManagerProbe()
    {
        ToDoManager = new ToDoManagerUnderTest(ToDoRepository, ToDoValidator, TimeProvider, Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its ToDos in.
    /// </summary>
    public ToDoRepositoryFake ToDoRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks ToDos with; every ToDo is valid until the test sets errors.
    /// </summary>
    public ToDoValidatorFake ToDoValidator { get; } = new();

    /// <summary>
    /// The clock the manager reads the current time from.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<ToDoManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public ToDoManagerUnderTest ToDoManager { get; }
}
