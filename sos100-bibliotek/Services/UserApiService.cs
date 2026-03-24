using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class UserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Autentiserar användaren och returnerar användarprofil vid lyckad inloggning.
    /// </summary>
    public async Task<UserProfileDto?> LoginAndGetUserAsync(string username, string password)
    {
        var payload = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", payload);

        if (response.IsSuccessStatusCode)
        {
            // Deserialiserar responsen till UserProfileDto för att inkludera UserId
            return await response.Content.ReadFromJsonAsync<UserProfileDto>();
        }

        return null;
    }

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