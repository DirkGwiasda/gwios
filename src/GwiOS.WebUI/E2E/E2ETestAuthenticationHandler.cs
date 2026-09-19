using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GwiOS.WebUI.E2E;

/// <summary>
/// Authenticates every request as a fixed test user without contacting Keycloak. <see cref="Program"/> registers
/// this scheme only when the hosting environment is named "E2E", so Playwright end-to-end tests can drive
/// protected pages without real credentials; it is never active in Development or Production.
/// </summary>
internal sealed class E2ETestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    /// <summary>
    /// The name of this authentication scheme, used both to register and to select it.
    /// </summary>
    public const string SchemeName = "E2ETest";

    /// <summary>
    /// The <c>preferred_username</c> claim value of the fixed test user, matching the claim type the real Keycloak
    /// sign-in uses.
    /// </summary>
    public const string TestUserName = "e2e-test-user";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        ClaimsIdentity identity = new([new Claim("preferred_username", TestUserName)], SchemeName);
        AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
