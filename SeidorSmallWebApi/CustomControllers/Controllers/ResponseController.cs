using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using SAPbobsCOM;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Entities;
using System.Data.Common;
using System.Text.Json;
using System.Threading;
namespace SeidorSmallWebApi.CustomControllers.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SLConnection _slConnection;
    private readonly ILogger<OrderController> _logger;
    public OrderController(IConfiguration configuration, SLConnection slConnection, ILogger<OrderController> logger)
    {
        _configuration = configuration;
        _slConnection = slConnection;
        _logger = logger;
    }
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ReceiveOrder([FromBody] Order orderRequest)
    {
        try
        {
            if (orderRequest == null)
                return BadRequest("Order data is required");
            if (orderRequest.CONF_ORDERLINESCollection == null || !orderRequest.CONF_ORDERLINESCollection.Any())
                return BadRequest("At least 1 line item is required");
            try
            {
                await _slConnection.Request("CONF_ORDERS").PostAsync(orderRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order to SAP");
                return StatusCode(500, $"Error sending order to SAP: {ex.Message}");
            }
            return Ok(new
            {
                message = $"Se han procesado las comandas con éxito.",
                orderData = JsonSerializer.Serialize(orderRequest)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return StatusCode(500, $"Internal server error while processing order: {ex.Message}");
        }
    }
}