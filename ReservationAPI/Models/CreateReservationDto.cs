namespace ReservationApi.Models;

/// <summary>
/// Användar- och bok-ID måste finnas i UserAPI respektive KatalogAPI.
/// ReservationAPI skapar inga användare eller böcker.
/// </summary>
public class CreateReservationDto
{
    public int UserId { get; set; }

    public int BookId { get; set; }

    public string Status { get; set; } = "Active";
}
