using System.Net.Http.Json;

namespace sos100_bibliotek.Services;

public class LoanService
{
    private readonly HttpClient _httpClient;

    public LoanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> BorrowBook(int bookId, string userId)
    {
        // Här ringer vi till LoanAPI:ets controller
        var response = await _httpClient.PostAsJsonAsync("api/Loan/borrow", new { BookId = bookId, UserId = userId });
        return response.IsSuccessStatusCode;
    }
}