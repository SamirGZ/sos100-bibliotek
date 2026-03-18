using Microsoft.AspNetCore.Mvc;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace Bibliotek.LoanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly LoanDbContext _context;

        public UserController(LoanDbContext context)
        {
            _context = context;
        }

        // Hämtar alla användare i den lokala databasen
        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users.ToList());
        }

        // Skapar en ny användare i SQLite-databasen
        [HttpPost]
        public IActionResult CreateUser([FromBody] User newUser)
        {
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return Ok(newUser);
        }
    }
}