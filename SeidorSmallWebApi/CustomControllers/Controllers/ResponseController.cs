using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Entities;
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
    public async Task<IActionResult> ReceiveOrder([FromBody] RecivedOrder[] IncomingOrderRequests)
    {
        if (IncomingOrderRequests == null || IncomingOrderRequests.Length == 0)
            return BadRequest("Order data is required");
        List<Order> orders = new List<Order>();
        foreach (RecivedOrder request in IncomingOrderRequests)
        {
            Order order = new Order();
            order.U_OrderId = request.id;
            order.U_CardCode = request.client;
            order.U_DocDate = request.orderDate;
            order.U_DocDueDate = request.dueDate;
            if (request.hasError == 0 || request.hasError == null)
            {
                order.U_Status = "C";
            }
            else
            {
                order.U_Status = "R";
                order.U_ErrorMsg = request.errorMessage;
            }
            int LineNum = 0;
            order.CONFORDERLINESCollection = new List<OrderLine>();
            foreach (RecivedOrderLines lines in request.lines)
            {
                OrderLine line = new OrderLine();
                line.U_OrderId = request.id;
                line.U_LineNum = LineNum;
                LineNum++;
                line.U_ItemCode = lines.itemCode;
                line.U_Quantity = lines.quantity;
                if (request.hasError == 0 || request.hasError == null)
                {
                    line.U_Batch = lines.batch;
                }
                order.CONFORDERLINESCollection.Add(line);
            }
            orders.Add(order);
        }
        try
        {


            foreach (var orderRequest in orders)
            {
                if (orderRequest.CONFORDERLINESCollection == null || !orderRequest.CONFORDERLINESCollection.Any())
                    return BadRequest("Each order must have at least 1 line item");

                try
                {
                    await _slConnection.Request("CONFORDERS").PostAsync(orderRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending order DocEntry {orderRequest.U_OrderId} to SAP");
                    return StatusCode(500, $"Error sending order DocEntry {orderRequest.U_OrderId} to SAP: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = $"S'han processat {orders.Count()} comandes amb èxit.",
                orders = orders.Select(o => o.U_OrderId).ToArray()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing orders");
            return StatusCode(500, $"Internal server error while processing orders: {ex.Message}");
        }
    }

}