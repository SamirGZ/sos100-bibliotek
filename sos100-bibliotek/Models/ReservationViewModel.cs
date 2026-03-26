namespace sos100_bibliotek.Models;
using sos100_bibliotek.Models;

public class ReservationViewModel
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int UserId { get; set; }

    public DateTime ReservationDate { get; set; }

    public string Status { get; set; } = "Active";
}