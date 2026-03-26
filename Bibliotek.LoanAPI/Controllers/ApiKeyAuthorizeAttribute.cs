using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Bibliotek.LoanAPI.Controllers;

// Vårt custom-filter för säkerhet. Kan sättas på hela klassen eller specifika metoder.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private const string HeaderName = "X-Api-Key";
    private readonly string? _configKey;

    public ApiKeyAuthorizeAttribute(string? configKey = null)
    {
        _configKey = configKey;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Vi måste hämta config via RequestServices här eftersom vanliga DI-konstruktorer 
        // inte fungerar rakt av inuti attribut.
        var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        if (configuration == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Letar efter vår inställning (standard är ApiSettings:LoanApiKey som vi satt i Azure)
        var expectedApiKey = (_configKey ?? "ApiSettings:LoanApiKey").Trim();
        var expected = configuration[expectedApiKey];

        if (string.IsNullOrWhiteSpace(expected))
        {
            // Fail-safe: Om vi glömt konfigurera nyckeln i molnet är det säkrare att 
            // blockera allt (401) än att råka lämna API:et öppet för alla.
            context.Result = new UnauthorizedResult();
            return;
        }

        // Kollar om frontenden skickade med X-Api-Key i sin request header
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedValues))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Jämför frontendens nyckel med Azures nyckel
        var provided = providedValues.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(provided) || !string.Equals(provided, expected, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedResult(); // Fel nyckel = sparka ut
            return;
        }
        
        // Når koden hit så stämde nyckeln, och anropet tillåts gå vidare till controllern!
    }
}