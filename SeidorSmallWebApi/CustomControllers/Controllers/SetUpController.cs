using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEIDOR_SLayer;

using SeidorSmallWebApi.Routes;
using System.Threading.Tasks;

namespace SeidorSmallWebApi.Controllers
{
    [ApiController]
    [Authorize] // Protegeix la endpoint perquè només pugui ser cridada per usuaris autenticats
    [Route($"{ApiRoutes.BaseRoute}/[controller]")]
    public class SetupController : ControllerBase
    {
       

        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            try
            {
               
                return Ok(new { message = "Initialization completed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
