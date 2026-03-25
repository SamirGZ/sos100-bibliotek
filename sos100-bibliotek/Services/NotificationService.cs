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

    public async Task CreateNotificationAsync(string message, int userId, string username)
    {
        var client = _httpClientFactory.CreateClient("NotificationsAPI");
        await client.PostAsJsonAsync("api/notifications", new { 
            Message = message, 
            UserId = userId, 
            Username = username, 
            IsRead = false 
        });
    }

    public async Task DeleteNotificationAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NotificationsAPI");
        await client.DeleteAsync($"api/notifications/{id}");
    }

    public async Task MarkAsReadAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("NotificationsAPI");
        // Uppdaterar status till läst
        await client.PutAsJsonAsync($"api/notifications/{id}", new { Message = "Markerad som läst", IsRead = true });
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
                        $"Påminnelse: Lånet för användare {loan.Username} är försenat.",
                        loan.UserId,
                        loan.Username
                    );
                }
            }
        }
        catch (Exception) { }
    }
}

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public bool IsReturned { get; set; }
}