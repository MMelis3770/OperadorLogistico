using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SeidorSmallWebApi.Routes;
using SeidorSmallWebApi.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SEIDOR_DecrypterLibrary;

namespace SeidorSmallWebApi.Controllers;

[ApiController]
[Authorize]
[Route($"{ApiRoutes.BaseRoute}/[controller]")]
[Produces("application/json")]
public class LoginController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly Login _login;

    public LoginController(IConfiguration configuration, SeiDecoder decoder)
    {
        _configuration = configuration;
        var jwtString = decoder.DecryptString(configuration.GetSection("Jwt").GetValue<string>("EncryptedUser"));
        var jwtJson = JsonDocument.Parse(jwtString);
        _login = new Login()
        {
            UserName = jwtJson.RootElement.GetProperty("AuthenticationUser").GetString(),
            Key = jwtJson.RootElement.GetProperty("AuthenticationKey").GetString()
        };
    }

    /// <summary>
    /// Grants access token to API
    /// </summary>
    /// <response code="200">Access Token</response>
    /// <response code="401">Incorrect User/password, unauthorized</response>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 401)]
    public ActionResult Login(Login login)
    {
        if (login.UserName == _login.UserName && login.Key == _login.Key)
        {
            // TODO: Fecha expiración token? Para que el usuario externo pueda validar
            string response = GenerateTokenLogin(login);
            return Ok(new { Token = response });
        }
        else
        {
            return Unauthorized("Incorrect username/password");
        }
    }

    private string GenerateTokenLogin(Login login)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var encodedAuthKey = Encoding.ASCII.GetBytes(login.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, login.UserName)
            }),
            Expires = DateTime.UtcNow.AddHours(1 + _configuration.GetValue<int>("AuthentificationTokenDuration")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(encodedAuthKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
