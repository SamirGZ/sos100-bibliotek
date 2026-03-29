using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class UserApiService
{
    private readonly HttpClient _httpClient;

    
    public UserApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<UserViewModel?> LoginAndGetUserAsync(string username, string password)
    {
        // Skickar credentials. Måste skickas som ett anonymt objekt för att matcha API:ets förväntade JSON.
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });

        if (response.IsSuccessStatusCode)
        {
            // Inloggningen lyckades (200 OK). Vi läser ut och mappar JSON-svaret till vår egen UserViewModel 
            // så vi kan spara datan i en cookie/session senare.
            return await response.Content.ReadFromJsonAsync<UserViewModel>();
        }
        return null; // Returnerar null om inloggningen misslyckas (t.ex. fel lösenord eller 404)
    }

    /// <summary>Samma UserAPI-instans som inloggning — ger rätt användarnamn per id i vyn.</summary>
    public async Task<UserApiProfileResponse?> GetUserByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/users/{id}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserApiProfileResponse>();
    }

    public async Task<UserViewModel?> GetUserAsync(string username)
    {
        // Hämtar specifik användare. 
        var response = await _httpClient.GetAsync($"api/auth/{username}");
    
        // Avbryter direkt och returnerar null om API:et inte hittar användaren
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<UserViewModel>();
    }

    public async Task<bool> RegisterAsync(string username, string password, string email)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { username, password, email });
    
        // För registrering bryr vi oss inte om att läsa ut någon JSON-data. 
        // Vi returnerar bara true/false så att kontrollern vet om den ska visa success-meddelande eller inte.
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> DeleteUserAsync(string username)
    {
        var response = await _httpClient.DeleteAsync($"api/auth/{username}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePasswordAsync(string username, string newPassword)
    {
        var response = await _httpClient.PutAsJsonAsync("api/auth/update-password", new { username, newPassword });
        return response.IsSuccessStatusCode;
    }
}