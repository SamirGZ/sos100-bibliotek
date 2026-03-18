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
}