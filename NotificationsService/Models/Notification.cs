namespace NotificationsService.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty; // Lägg till denna för att spara namnet permanent
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } // Detta fixar det röda felet i Controllern
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}