namespace sos100_bibliotek.Models;

public class ReservationViewModel
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int UserId { get; set; }
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public string BookTitle { get; set; } = "";
    public string BookAuthor { get; set; } = "";
}