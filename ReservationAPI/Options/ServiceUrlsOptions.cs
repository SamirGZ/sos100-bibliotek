namespace ReservationApi.Options;

public class ServiceUrlsOptions
{
    public const string SectionName = "ServiceUrls";

    /// <summary>Bas-URL till UserAPI (utan avslutande snedstreck), t.ex. Azure App Service.</summary>
    public string UserApi { get; set; } = string.Empty;

    /// <summary>Bas-URL till KatalogAPI / Catalogue-service.</summary>
    public string CatalogueApi { get; set; } = string.Empty;
}
