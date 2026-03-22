using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class CatalogueService
{
    private HttpClient _httpClient;

    public CatalogueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5149/");
    }

    public async Task<BookCatalogue[]?> GetBookCatalogue()
    {
        var bookcatalogue = await _httpClient.GetFromJsonAsync<BookCatalogue[]>("api/Books");

        if (bookcatalogue == null)
        {
            return Array.Empty<BookCatalogue>();
        }
        return bookcatalogue;
    }

    public async Task<BookCatalogue> UpdateBookById(int Id)
    {
        return await _httpClient.GetFromJsonAsync<BookCatalogue>($"api/UpdateBook/{Id}");
    }

    public async Task<bool> UpdateBook(BookCatalogue bookCatalogue)
    {
        var Respons = await _httpClient.PutAsJsonAsync($"api/UpdateBook/{bookCatalogue.Id}", bookCatalogue);
        
        return Respons.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBookById(int Id)
    {
        var Respons = await _httpClient.DeleteAsync($"api/DeleteBook/{Id}");
        
        return Respons.IsSuccessStatusCode;
    }
}