namespace Bibliotek.LoanAPI.Models; 

public class LoanEvent
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.Now;

    public Loan Loan { get; set; } = null!; 
}