namespace Bibliotek.LoanAPI.Models; 

// Huvudentitet för utlåningslogiken som lagras i SQLite-databasen.
public class Loan
{
    public int Id { get; set; }
    
    // Vi sparar användardata direkt här för att undvika onödiga nätverksanrop (joins) mot UserAPI varje gång lånen visas.
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    
    // Används som en flagga för att frigöra boken utan att radera posten (Soft delete-principen).
    public bool IsReturned { get; set; }
    
    // Datum styrs helt från API-kontrollern för att förhindra att klienten manipulerar lånetiden.
    public DateTime LoanDate { get; set; } 
    public DateTime ReturnDate { get; set; } 
    
    // Navigeringsegenskap för Entity Framework för att kunna dra ut tillhörande händelseloggar (1-till-många relation).
    public List<LoanEvent>? History { get; set; } = new();
}