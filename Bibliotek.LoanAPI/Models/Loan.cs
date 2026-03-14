namespace Bibliotek.LoanAPI.Models;

public class Loan
{
    public int Id { get; set; }             // Lånets unika ID i databasen
    public int BookId { get; set; }         // Vilken bok som lånas (Kopplas sen till Catalog API)
    public int UserId { get; set; }         // Vem som lånar (Kopplas sen till User API)
    public DateTime LoanDate { get; set; }  // När boken lånades
    public DateTime ReturnDate { get; set; } // När boken senast ska lämnas in
    public bool IsReturned { get; set; }    // Är boken tillbakalämnad? (True/False)
}