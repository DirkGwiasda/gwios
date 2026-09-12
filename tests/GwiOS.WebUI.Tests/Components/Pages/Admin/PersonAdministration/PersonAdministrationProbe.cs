using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Base of the <c>PersonAdministration</c> tests: provides an in-memory person manager and status message service.
/// </summary>
public abstract class PersonAdministrationProbe : BunitContext
{
    protected PersonAdministrationProbe()
    {
        Services.AddSingleton<IPersonManager>(PersonManager);
        Services.AddSingleton<IStatusMessageService>(StatusMessages);
    }

    /// <summary>
    /// The person manager the page works with.
    /// </summary>
    protected PersonManagerFake PersonManager { get; } = new();

    /// <summary>
    /// The status message service the page publishes to.
    /// </summary>
    protected StatusMessageServiceFake StatusMessages { get; } = new();

    /// <summary>
    /// Renders the page under test.
    /// </summary>
    protected IRenderedComponent<PersonAdministrationUnderTest> RenderPage()
        => Render<PersonAdministrationUnderTest>();

    /// <summary>
    /// Enters the given name and short name into the form of the page and clicks the add button.
    /// </summary>
    protected static void AddPerson(IRenderedComponent<PersonAdministrationUnderTest> page, string name, string shortName)
    {
        page.Find("input[aria-label='Name der neuen Person']").Input(name);
        page.Find("input[aria-label='Kurzname der neuen Person']").Input(shortName);
        page.Find(".gwios-form-panel button").Click();
    }
}
