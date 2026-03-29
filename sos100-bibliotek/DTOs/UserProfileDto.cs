namespace sos100_bibliotek.Models;

public class UserProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; } // ✅ NY RAD – matchar "userId" från AuthController
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}