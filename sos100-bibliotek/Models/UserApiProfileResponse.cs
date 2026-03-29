using System.Text.Json.Serialization;

namespace sos100_bibliotek.Models;

/// <summary>Svar från UserAPI GET api/users/{id} (camelCase: id, username, email).</summary>
public class UserApiProfileResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
