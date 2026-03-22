namespace sos100_bibliotek.Models;

public class UserResponse
{
    // Om ditt UserAPI skickar JSON med "id", kan du behöva detta:
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
}