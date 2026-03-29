namespace ReservationApi.Models;

/// <summary>Svar från KatalogAPI <c>GET /api/Books/{id}</c>.</summary>
public class CatalogueBook
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}
