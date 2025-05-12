using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using SAPbobsCOM;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Entities;
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

    [HttpPost]
    public async Task<IActionResult> ReceiveOrder([FromBody] Order orderRequest)
    {
        try
        {
            if (orderRequest == null)
                return BadRequest("Order data is required");


            if (orderRequest.lines == null || !orderRequest.lines.Any())
                return BadRequest("At least 1 line item is required");

            var sapOrder = TransformToSapOrder(orderRequest);

            // Enviar a SAP
            var result = await SendToSAP();

            if (result.errorCount > 0)
            {
                return BadRequest($"Failed to process order: {result.errorMessages}");
            }

            // Resposta exitosa amb detalls de l'ordre processada
            return Ok(new
            {
                message = $"Se han procesado {result.successCount} comandas con éxito. JSON generado para API:",
                orderData = JsonSerializer.Serialize(orderRequest)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return StatusCode(500, "Internal server error while processing order");
        }
    }

    private object TransformToSapOrder(Order order)
    {
        // Transformació del model de l'API al model de SAP
        var documentLines = order.lines.Select(line => new
        {
            DocEntry = line.DocEntry,
            LineNum = line.LineNum,
            ItemCode = line.ItemCode,
            Quantity = line.Quantity,
            Batch = line.Batch
        });
        return new
        {
            DocEntry = order.DocEntry,
            CardCode = order.CardCode,
            DocDate = order.DocDate,
            DocDueDate = order.DocDueDate,
            Status = order.Status,
            ErrorMsg = order.ErrorMsg
        };
    }

    [NonAction]
    public async Task<(int successCount, int errorCount, string errorMessages)> SendToSAP()
    {
        string QueryOrder = "INSERT INTO[@CONF_ORDERS] (DocEntry, CardCode, DocDate, DocDueDate, Status, ErrorMsg) VALUES('X', 'X', '2025-05-12', '2025-05-20', 'C', '')";


        
        string QueryOrderLines = "INSERT INTO[@CONF_ORDERLINES] (DocEntry, LineNum, ItemCode, Quantity, Batch) VALUES(Mateix DOC entry que a dal, 0, 'A001', 5, 'B001')";
    }
}

