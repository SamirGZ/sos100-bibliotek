using System.Net.Http.Json;
using sos100_bibliotek.Models; // Se till att du har UserResponse-klassen i denna mapp

public class UserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // UPPDATERAD: Returnerar nu objektet så att Controllern får tag i ID:t
    public async Task<UserResponse?> LoginAsync(string username, string password)
    {
        var payload = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", payload);
        
        if (response.IsSuccessStatusCode)
        {
            // Här läser vi ut ID och Username som din klasskamrats API skickar
            return await response.Content.ReadFromJsonAsync<UserResponse>();
        }
        return null;
    }

    // --- HÄRIFRÅN OCH NEDÅT ÄR ALLT ORÖRT FÖR DIN KLASSKAMRAT ---

    public async Task<bool> RegisterAsync(string username, string password, string email)
    {
        var payload = new { username, password, email };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", payload);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<UserProfileDto?> GetUserAsync(string username)
    {
        var response = await _httpClient.GetAsync($"/api/auth/{username}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<UserProfileDto>();
    }

    public async Task<bool> DeleteUserAsync(string username)
    {
        var response = await _httpClient.DeleteAsync($"/api/auth/{username}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePasswordAsync(string username, string newPassword)
    {
        var payload = new { username, newPassword };
        var response = await _httpClient.PutAsJsonAsync("/api/auth/update-password", payload);
        return response.IsSuccessStatusCode;
    }
}