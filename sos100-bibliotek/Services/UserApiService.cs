public class UserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var payload = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", payload);
        return response.IsSuccessStatusCode;
    }
}