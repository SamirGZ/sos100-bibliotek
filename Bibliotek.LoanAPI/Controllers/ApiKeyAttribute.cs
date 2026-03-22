using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Bibliotek.LoanAPI;

namespace Bibliotek.LoanAPI
{
    // Denna klass skapar själva [ApiKey]-attributet som vi använder i Controllern
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "X-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Kolla om nyckeln finns i Headern 
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult() { StatusCode = 401, Content = "API-nyckel saknas!" };
                return;
            }

            // 2. Hämta den rätta nyckeln från appsettings.json
            var appSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>("ApiKey");

            // 3. Jämför nycklarna
            if (apiKey == null || !apiKey.Equals(extractedApiKey))
            {
                context.Result = new ContentResult() { StatusCode = 403, Content = "Fel API-nyckel!" };
                return;
            }

            await next();
        }
    }
}