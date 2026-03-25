using System.Net.Http.Json;

namespace sos100_bibliotek.Services;

public class NotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationsAPI");
            var result = await client.GetFromJsonAsync<List<NotificationDto>>("api/notifications");
            return result ?? new List<NotificationDto>();
        }
        catch (Exception)
        {
            return new List<NotificationDto>();
        }
    }

    public async Task CreateNotificationAsync(string message, int userId)
    {
        var client = _httpClientFactory.CreateClient("NotificationsAPI");
        await client.PostAsJsonAsync("api/notifications", new { Message = message, UserId = userId, IsRead = false });
    }

    public async Task DeleteNotificationAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NotificationsAPI");
        await client.DeleteAsync($"api/notifications/{id}");
    }

    public async Task MarkAsReadAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NotificationsAPI");
        await client.PutAsJsonAsync($"api/notifications/{id}", new { Message = "", IsRead = true });
    }

    public async Task CheckOverdueLoansAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var loans = await client.GetFromJsonAsync<List<LoanDto>>("http://localhost:5029/api/loan");

            if (loans == null) return;

            foreach (var loan in loans)
            {
                if (!loan.IsReturned && loan.ReturnDate < DateTime.Now)
                {
                    await CreateNotificationAsync(
                        $"Påminnelse: Lån #{loan.Id} för användare {loan.UserId} är försenat sedan {loan.ReturnDate:d}",
                        loan.UserId
                    );
                }
            }
        }
        catch (Exception)
        {
            // Loan API är inte tillgänglig just nu
        }
    }
}

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public DateTime ReturnDate { get; set; }
    public bool IsReturned { get; set; }
}