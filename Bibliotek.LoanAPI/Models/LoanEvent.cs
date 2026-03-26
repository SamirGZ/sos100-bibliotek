namespace Bibliotek.LoanAPI.Models; 

// Fungerar som en händelselogg (audit trail) för varje lån för att kunna spåra exakt när status ändrades.
public class LoanEvent
{
    public int Id { get; set; }
    
    // Främmande nyckel (Foreign Key) som kopplar händelsen till ett specifikt lån i databasen.
    public int LoanId { get; set; }
    
    // Beskriver själva händelsen, t.ex. "Lån skapat" eller "Bok återlämnad".
    public string Action { get; set; } = string.Empty;
    
    // Sätts direkt vid instansiering så att loggningen får en garanterat exakt tidsstämpel.
    public DateTime EventDate { get; set; } = DateTime.Now;

    // Tvingande navigeringsegenskap för att Entity Framework ska förstå relationen tillbaka till Loan.
    public Loan Loan { get; set; } = null!; 
}