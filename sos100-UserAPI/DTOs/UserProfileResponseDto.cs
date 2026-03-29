namespace sos100_UserAPI.DTOs;

/// <summary>Säkert svar för andra tjänster (t.ex. ReservationAPI) utan lösenord.</summary>
public class UserProfileResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
