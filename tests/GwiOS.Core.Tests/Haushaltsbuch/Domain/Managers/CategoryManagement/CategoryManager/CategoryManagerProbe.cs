using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using CategoryManagerUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryManager;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryManager;

/// <summary>
/// Runs a <c>CategoryManager</c> against an in-memory repository, a configurable validator and a recording logger,
/// and gives the tests access to all three.
/// </summary>
internal sealed class CategoryManagerProbe
{
    public CategoryManagerProbe()
    {
        CategoryManager = new CategoryManagerUnderTest(CategoryRepository, CategoryValidator, Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its categories in.
    /// </summary>
    public CategoryRepositoryFake CategoryRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks categories with; every category is valid until the test sets errors.
    /// </summary>
    public CategoryValidatorFake CategoryValidator { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<CategoryManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public CategoryManagerUnderTest CategoryManager { get; }
}
