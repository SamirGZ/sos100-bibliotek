using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userService.AuthenticateAsync(dto.Username, dto.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new { message = "Login successful", userId = user.Id, username = user.Username });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var success = await _userService.RegisterAsync(dto.Username, dto.Password, dto.Email);
        if (!success)
            return BadRequest(new { message = "Username already exists" });

        return Ok(new { message = "User registered successfully" });
    }
    
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        var user = await _userService.GetUserAsync(username);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new { user.Username, user.Email });
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteUser(string username)
    {
        var success = await _userService.DeleteUserAsync(username);
        if (!success)
            return NotFound(new { message = "User not found" });

        return Ok(new { message = "User deleted successfully" });
    }

    [HttpPut("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var success = await _userService.UpdatePasswordAsync(dto.Username, dto.NewPassword);
        if (!success)
            return NotFound(new { message = "User not found" });

        return Ok(new { message = "Password updated successfully" });
    }
    
}