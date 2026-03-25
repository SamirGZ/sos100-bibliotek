namespace Bibliotek.LoanAPI.Models; 
public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public bool IsReturned { get; set; }
    public DateTime LoanDate { get; set; } // Krävs för din Controller
    public DateTime ReturnDate { get; set; } // Krävs för din Controller
    
    public List<LoanEvent>? History { get; set; } = new();
}