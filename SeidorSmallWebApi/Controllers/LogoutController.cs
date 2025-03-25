using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Extensions;
using SeidorSmallWebApi.Routes;

namespace SeidorSmallWebApi.Controllers;

[ApiController]
[Authorize]
[Route($"{ApiRoutes.BaseRoute}/[controller]")]
public class LogoutController : ControllerBase
{

    private readonly SLConnection _serviceLayerConnection;
    private readonly ITokenManager _tokenManager;
    public LogoutController(SLConnection serviceLayerConnection, ITokenManager tokenManager)
    {
        _serviceLayerConnection = serviceLayerConnection;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Logout from the API, kill Service Layer connection if active.
    /// </summary>
    
    [HttpPost]
    public async Task<ActionResult> Logout()
    {
        Task a = _serviceLayerConnection.LogoutAsync();
        Task b = _tokenManager.DeactivateCurrentAsync();
        await Task.WhenAll(a, b);
        return Ok();
    }
}
