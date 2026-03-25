using System.Text.Json.Serialization;

namespace sos100_bibliotek.Models
{
    public class UserViewModel
    {
        [JsonPropertyName("userId")]
        public int Id { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")] // Detta gör att den slutar lysa rött i vyn
        public string Email { get; set; } = string.Empty;
    }
}