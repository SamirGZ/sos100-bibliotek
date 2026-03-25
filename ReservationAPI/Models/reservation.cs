namespace ReservationApi.Models;

public class Reservation
{
    public int Id { get; set; }

    // Koppling till bok i katalogen
    public int BookId { get; set; }

    // Koppling till användaren som reserverar
    public int UserId { get; set; }

    public DateTime ReservationDate { get; set; } = DateTime.Now;

    public string Status { get; set; } = "Active";
}