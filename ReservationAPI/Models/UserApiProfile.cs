namespace ReservationApi.Models;

/// <summary>Svar från UserAPI <c>GET /api/users/{id}</c>.</summary>
public class UserApiProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
