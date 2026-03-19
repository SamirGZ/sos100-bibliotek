namespace sos100_bibliotek.Models;

public class BookCatalogue
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Publisher { get; set; } = "";
    public int Year { get; set; }
    public string Isbn { get; set; } = "";
    public int Pages { get; set; }
    public string Language { get; set; } = "";
}
