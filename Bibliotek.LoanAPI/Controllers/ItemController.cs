using Microsoft.AspNetCore.Mvc;

namespace sos100_bibliotek.Controllers
{
    // Den här raden bestämmer att URL:en blir "api/item"
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        // Den här raden säger att funktionen ska svara på GET-anrop
        [HttpGet]
        public IActionResult GetItems()
        {
            // Vi returnerar ett meddelande med statuskod 200 (OK)
            return Ok(new { message = "Hej, C#-controllern fungerar!" });
        }
    }
}