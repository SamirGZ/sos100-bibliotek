namespace Bibliotek.LoanAPI.Models;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty; // LÄGG TILL DENNA RAD!
    public int UserId { get; set; } 
    public DateTime LoanDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public List<LoanEvent> History { get; set; } = new List<LoanEvent>();
}