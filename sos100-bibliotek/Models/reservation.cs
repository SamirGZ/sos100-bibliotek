namespace sos100_bibliotek.Models;

public class Reservation
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int UserId { get; set; }

    public DateTime ReservationDate { get; set; }

    public string Status { get; set; } = "Active";
}