using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class CatalogueService
{
    private readonly HttpClient _httpClient;

    public CatalogueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookCatalogue[]?> GetBookCatalogue()
    {
        try 
        {
            return await _httpClient.GetFromJsonAsync<BookCatalogue[]>("api/Books");
        }
        catch { return Array.Empty<BookCatalogue>(); }
    }

    public async Task<BookCatalogue?> UpdateBookById(int id)
    {
        try {
            return await _httpClient.GetFromJsonAsync<BookCatalogue>($"api/Books/{id}");
        } catch { return null; }
    }

    public async Task<bool> UpdateBook(BookCatalogue bookCatalogue)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Books/{bookCatalogue.Id}", bookCatalogue);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBookById(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Books/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateBook(BookCatalogue bookCatalogue)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Books", bookCatalogue);
        return response.IsSuccessStatusCode;
    }
}