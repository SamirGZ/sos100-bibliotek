using System.Net.Http.Json;

namespace sos100_bibliotek.Services;

public class LoanService
{
    private readonly HttpClient _httpClient;

    // Tar in HttpClient (konfigurerad via IHttpClientFactory i Program.cs för att slippa port-strul)
    public LoanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> BorrowBook(int bookId, string userId)
    {
        // Skickar ett anonymt objekt som payload för att matcha det som LoanAPI förväntar sig
        var response = await _httpClient.PostAsJsonAsync("api/Loan/borrow", new { BookId = bookId, UserId = userId });
        
        // Returnerar bara true/false beroende på om API:et svarade med 2xx-statuskod (lyckades)
        return response.IsSuccessStatusCode;
    }
}