namespace Catalogue_service.Models;

public class BookCatalogue
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Publisher { get; set; } = "";
    public int Year { get; set; }
}