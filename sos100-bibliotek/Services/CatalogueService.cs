using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class CatalogueService
{
    private HttpClient _httpClient;

    public CatalogueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookCatalogue[]> GetBookCatalogue()
    {
        var bookcatalogue = await _httpClient.GetFromJsonAsync<BookCatalogue[]>("BookCatalogue");

        if (bookcatalogue == null)
        {
            return [];
        }
        return bookcatalogue;
    }
}