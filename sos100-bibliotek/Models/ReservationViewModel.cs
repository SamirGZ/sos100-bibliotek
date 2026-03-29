using System.Text.Json.Serialization;

namespace sos100_bibliotek.Models;

public class ReservationViewModel
{
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("bookId")]
    public int ItemId { get; set; }
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public string BookTitle { get; set; } = "";
    public string Author { get; set; } = "";
}