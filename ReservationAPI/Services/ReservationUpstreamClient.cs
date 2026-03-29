using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReservationApi.Models;
using ReservationApi.Options;

namespace ReservationApi.Services;

public class ReservationUpstreamClient : IReservationUpstreamClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceUrlsOptions _urls;

    public ReservationUpstreamClient(IHttpClientFactory httpClientFactory, IOptions<ServiceUrlsOptions> urls)
    {
        _httpClientFactory = httpClientFactory;
        _urls = urls.Value;
    }

    public async Task<UserApiProfile?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _urls.UserApi?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return null;

        var client = _httpClientFactory.CreateClient(nameof(ReservationUpstreamClient));
        var response = await client.GetAsync($"{baseUrl}/api/users/{userId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserApiProfile>(cancellationToken: cancellationToken);
    }

    public async Task<CatalogueBook?> GetBookAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _urls.CatalogueApi?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return null;

        var client = _httpClientFactory.CreateClient(nameof(ReservationUpstreamClient));
        var response = await client.GetAsync($"{baseUrl}/api/Books/{bookId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CatalogueBook>(cancellationToken: cancellationToken);
    }
}
