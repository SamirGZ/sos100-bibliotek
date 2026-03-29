using Microsoft.AspNetCore.Mvc;
using sos100_UserAPI.DTOs;

namespace sos100_UserAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfileResponseDto>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserProfileResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        });
    }
}
