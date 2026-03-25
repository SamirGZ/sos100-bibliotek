using System.Net.Http.Json;

namespace sos100_bibliotek.Services;

// 1. DTO-klasser (Modeller för data från API:erna)
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

// 2. Själva tjänsten
public class NotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public NotificationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationsAPI");
            return await client.GetFromJsonAsync<List<NotificationDto>>("api/notifications") ?? new();
        }
        catch { return new List<NotificationDto>(); }
    }

    public async Task CheckOverdueLoansAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var loanApiUrl = _configuration["ServiceUrls:LoanApi"];
            var loans = await client.GetFromJsonAsync<List<LoanDto>>($"{loanApiUrl}/api/loan");

            if (loans == null) return;

            foreach (var loan in loans)
            {
                if (!loan.IsReturned && loan.ReturnDate < DateTime.Now)
                {
                    await CreateNotificationAsync(
                        $"PÅMINNELSE: Lånet är försenat. Vänligen returnera boken.",
                        loan.UserId,
                        loan.Username
                    );
                }
            }
        }
        catch (Exception ex) { Console.WriteLine("CheckOverdue Error: " + ex.Message); }
    }

    private async Task CreateNotificationAsync(string message, int userId, string username)
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
        await client.PutAsJsonAsync($"api/notifications/{id}", new { Id = id, IsRead = true });
    }
}