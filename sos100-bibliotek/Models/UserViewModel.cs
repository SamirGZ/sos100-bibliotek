using System.Text.Json.Serialization;

namespace sos100_bibliotek.Models
{
    // DTO (Data Transfer Object) som används för att ta emot och mappa JSON-data från UserAPI.
    public class UserViewModel
    {
        // Mappar camelCase från JSON-svaret till PascalCase i C# för att följa kodstandard.
        [JsonPropertyName("userId")]
        public int Id { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        // Binder fältet explicit för att serialiseringen ska fungera och undvika null-referenser i vyerna.
        [JsonPropertyName("email")] 
        public string Email { get; set; } = string.Empty;
    }
}