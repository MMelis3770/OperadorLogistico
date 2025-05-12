using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPbobsCOM;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Entities;
using System.Text.Json;

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

            if (orderRequest.Lines == null || !orderRequest.Lines.Any())
                return BadRequest("At least 1 line item is required");

            // Extract order lines
            var orderLines = orderRequest.Lines;

            // Create order header (without lines)
            var orderHeader = new OrderHeader
            {
                DocEntry = orderRequest.DocEntry,
                U_CardCode = orderRequest.U_CardCode,
                U_DocDate = orderRequest.U_DocDate,
                U_DocDueDate = orderRequest.U_DocDueDate,
                U_Status = orderRequest.U_Status,
                U_ErrorMsg = orderRequest.U_ErrorMsg
            };

            try
            {
                // Send order header to SAP
                await _slConnection.Request("CONF_ORDERS").PostAsync(orderHeader);

                // Send order lines to SAP
                await _slConnection.Request("CONF_ORDERLINES").PostAsync(orderLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order to SAP");
                return StatusCode(500, $"Error sending order to SAP: {ex.Message}");
            }

            return Ok(new
            {
                message = $"Se han procesado las comandas con éxito.",
                headerData = JsonSerializer.Serialize(orderHeader),
                linesData = JsonSerializer.Serialize(orderLines)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return StatusCode(500, $"Internal server error while processing order: {ex.Message}");
        }
    }
}