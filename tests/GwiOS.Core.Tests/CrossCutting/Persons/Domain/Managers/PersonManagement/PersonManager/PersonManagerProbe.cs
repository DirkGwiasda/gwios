using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using PersonManagerUnderTest = GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

/// <summary>
/// Runs a <c>PersonManager</c> against an in-memory repository, a configurable validator and a recording logger,
/// and gives the tests access to all three.
/// </summary>
internal sealed class PersonManagerProbe
{
    public PersonManagerProbe()
    {
        PersonManager = new PersonManagerUnderTest(PersonRepository, PersonValidator, Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its persons in.
    /// </summary>
    public PersonRepositoryFake PersonRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks persons with; every person is valid until the test sets errors.
    /// </summary>
    public PersonValidatorFake PersonValidator { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<PersonManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public PersonManagerUnderTest PersonManager { get; }
}
