using System.Net.Http.Json;
using System.Text.Json;

namespace sos100_bibliotek.Services;

// DTO – API:t serialiserar normalt camelCase; vi måste läsa case-insensitivt.
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

public class NotificationService
{
    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public NotificationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    /// <summary>
    /// Alltid full URL – fungerar oavsett HttpClient BaseAddress (vanlig felkälla i Azure).
    /// </summary>
    private string NotificationsRoot()
    {
        var url = _configuration["ServiceUrls:NotificationsApi"]?.Trim();
        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("Saknar ServiceUrls:NotificationsApi i konfiguration.");
        return url.TrimEnd('/');
    }

    private static async Task ThrowUnlessOk(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"Notifications API {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        var url = $"{NotificationsRoot()}/api/notifications";
        try
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetFromJsonAsync<List<NotificationDto>>(url, JsonReadOptions) ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hämtningsfel notiser: " + ex.Message);
            return new List<NotificationDto>();
        }
    }

    /// <summary>Hämtar en notis (används innan delete/mark-read så vi kan verifiera rätt användare).</summary>
    public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
    {
        var url = $"{NotificationsRoot()}/api/notifications/{id}";
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        await ThrowUnlessOk(response);
        return await response.Content.ReadFromJsonAsync<NotificationDto>(JsonReadOptions);
    }

    public async Task CheckOverdueLoansAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var loanApiUrl = _configuration["ServiceUrls:LoanApi"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(loanApiUrl)) return;

            var loans = await client.GetFromJsonAsync<List<LoanDto>>($"{loanApiUrl}/api/loan", JsonReadOptions);
            if (loans == null) return;

            foreach (var loan in loans)
            {
                if (!loan.IsReturned && loan.ReturnDate < DateTime.Now)
                {
                    await CreateNotificationAsync(
                        "PÅMINNELSE: Lånet är försenat. Vänligen returnera boken.",
                        loan.UserId,
                        loan.Username);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("CheckOverdue Error: " + ex.Message);
        }
    }

    private async Task CreateNotificationAsync(string message, int userId, string username)
    {
        var url = $"{NotificationsRoot()}/api/notifications";
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(url, new
        {
            Message = message,
            UserId = userId,
            Username = username,
            IsRead = false
        });
        await ThrowUnlessOk(response);
    }

    public async Task DeleteNotificationAsync(int id)
    {
        var client = _httpClientFactory.CreateClient();
        // GET /delete – fungerar där DELETE blockeras eller strular
        var url = $"{NotificationsRoot()}/api/notifications/{id}/delete";
        var response = await client.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;
        await ThrowUnlessOk(response);
    }

    public async Task MarkAsReadAsync(int id)
    {
        var client = _httpClientFactory.CreateClient();
        // GET /read – samma logik som POST, enklast för server→server på Azure
        var url = $"{NotificationsRoot()}/api/notifications/{id}/read";
        var response = await client.GetAsync(url);
        await ThrowUnlessOk(response);
    }
}
