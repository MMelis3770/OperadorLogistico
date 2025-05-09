using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    public async Task<IActionResult> ReceiveOrder([FromBody] Order orderRequest)
    {
        try
        {
            if (orderRequest == null)
                return BadRequest("Order data is required");

            // Validacions
            if (string.IsNullOrEmpty(orderRequest.client))
                return BadRequest("Client code is required");

            if (orderRequest.lines == null || !orderRequest.lines.Any())
                return BadRequest("At least 1 line item is required");

            // Transformar a format SAP
            var sapOrder = TransformToSapOrder(orderRequest);

            // Enviar a SAP
            var batchRequest = new SLBatchRequest(HttpMethod.Post, "Orders", sapOrder).WithReturnNoContent();
            var result = await SendToSAP(new List<SLBatchRequest> { batchRequest });

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
            ItemCode = line.itemCode,
            Quantity = line.quantity,
            BatchNumbers = !string.IsNullOrEmpty(line.batch) ? new[]
            {
                new { BatchNumber = line.batch, Quantity = line.quantity }
            } : null
        }).ToList();

        return new
        {
            CardCode = order.client,
            DocDate = order.orderDate,
            DocDueDate = order.dueDate,
            DocumentLines = documentLines
        };
    }

    public async Task<(int successCount, int errorCount, string errorMessages)> SendToSAP(List<SLBatchRequest> batchRequests)
    {
        try
        {
            HttpResponseMessage[] batchResult = await _slConnection.PostBatchAsync(batchRequests);

            int successCount = 0;
            int errorCount = 0;
            List<string> errorMessages = new List<string>();

            foreach (var response in batchResult)
            {
                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                }
                else
                {
                    errorCount++;
                    string errorContent = await response.Content.ReadAsStringAsync();
                    errorMessages.Add(errorContent);
                    _logger.LogError($"SAP API error: {errorContent}");
                }
            }

            return (successCount, errorCount, string.Join("; ", errorMessages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending data to SAP");
            return (0, batchRequests.Count, ex.Message);
        }
    }
}

