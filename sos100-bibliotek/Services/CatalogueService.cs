using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class CatalogueService
{
    private readonly HttpClient _httpClient;

    // Vi tar emot en färdig HttpClient som redan har rätt URL från Azure
    public CatalogueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookCatalogue[]?> GetBookCatalogue()
    {
        try 
        {
            // Anropar din BooksController
            var bookcatalogue = await _httpClient.GetFromJsonAsync<BookCatalogue[]>("api/Books");
            return bookcatalogue ?? Array.Empty<BookCatalogue>();
        }
        catch (Exception)
        {
            return Array.Empty<BookCatalogue>();
        }
    }

    public async Task<BookCatalogue?> UpdateBookById(int Id)
    {
        return await _httpClient.GetFromJsonAsync<BookCatalogue>($"api/UpdateBook/{Id}");
    }

    public async Task<bool> UpdateBook(BookCatalogue bookCatalogue)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UpdateBook/{bookCatalogue.Id}", bookCatalogue);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBookById(int Id)
    {
        var response = await _httpClient.DeleteAsync($"api/DeleteBook/{Id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateBook(BookCatalogue bookCatalogue)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/CreateBook", bookCatalogue);
        return response.IsSuccessStatusCode;
    }
}