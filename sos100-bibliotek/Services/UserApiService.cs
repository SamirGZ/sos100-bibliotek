using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class UserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<UserViewModel?> LoginAndGetUserAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username, password });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserViewModel>();
        }
        return null;
    }

    public async Task<UserViewModel?> GetUserAsync(string username)
    {
        var response = await _httpClient.GetAsync($"/api/auth/{username}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<UserViewModel>();
    }

    public async Task<bool> RegisterAsync(string username, string password, string email)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new { username, password, email });
        return response.IsSuccessStatusCode;
    }

    // Dessa två rader fixar de 2 felen i ProfileController:
    public async Task<bool> DeleteUserAsync(string username)
    {
        var response = await _httpClient.DeleteAsync($"/api/auth/{username}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePasswordAsync(string username, string newPassword)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/auth/update-password", new { username, newPassword });
        return response.IsSuccessStatusCode;
    }
}