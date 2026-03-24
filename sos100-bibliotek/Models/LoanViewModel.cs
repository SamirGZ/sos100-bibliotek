using System.Text.Json.Serialization;

namespace sos100_bibliotek.Models;

public class LoanViewModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("bookId")]
    public int BookId { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("loanDate")]
    public DateTime LoanDate { get; set; }

    // Denna används för att visa titeln på skärmen, 
    // även om API:et bara skickar ID:t kan vi behöva mappa den.
    public string BookTitle { get; set; } = "Hämtar titel...";
}