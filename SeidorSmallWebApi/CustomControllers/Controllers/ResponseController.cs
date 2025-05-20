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
    public async Task<IActionResult> ReceiveOrder([FromBody] Order[] orderRequests)
    {
        try
        {
            if (orderRequests == null || orderRequests.Length == 0)
                return BadRequest("Order data is required");

            foreach (var orderRequest in orderRequests)
            {
                if (orderRequest.CONF_ORDERLINESCollection == null || !orderRequest.CONF_ORDERLINESCollection.Any())
                    return BadRequest("Each order must have at least 1 line item");

                try
                {
                    await _slConnection.Request("CONF_ORDERS").PostAsync(orderRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending order DocEntry {orderRequest.U_OrderId} to SAP");
                    return StatusCode(500, $"Error sending order DocEntry {orderRequest.U_OrderId} to SAP: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = $"S'han processat {orderRequests.Length} comandes amb èxit.",
                orders = orderRequests.Select(o => o.U_OrderId).ToArray()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing orders");
            return StatusCode(500, $"Internal server error while processing orders: {ex.Message}");
        }
    }

}