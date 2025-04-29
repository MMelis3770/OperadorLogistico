using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using SAPbobsCOM;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Entities;

namespace SeidorSmallWebApi.CustomControllers.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ResponseController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SLConnection _slConnection;
    List<ResponseHeader> _orders;
    int Errors;
    
    public ResponseController(IConfiguration configuration,SLConnection sLConnection)
    {
        _configuration = configuration;
        _slConnection = sLConnection;
        _orders = new List<ResponseHeader>();
        Errors = 0;
    }
    [HttpPost]
    public async Task<IActionResult> ReciveResponse([FromBody] List<ResponseHeader> response)
    {
        if (response == null)
            return BadRequest("Response data is required");

        List<SLBatchRequest> batches = new List<SLBatchRequest>();

        foreach (ResponseHeader order in response)
        {
            if (string.IsNullOrEmpty(order.CardCode))
                return BadRequest("Customer name is required");

            if (order.Lines.Count <= 0)
                return BadRequest("At least 1 line is required");

            var request = new SLBatchRequest(HttpMethod.Post, "Orders", order).WithReturnNoContent();
            batches.Add(request);
        }

        await SendToSAP(batches);

        if (Errors == 0)
        {
            return Created();
        }
        else
        {
            return BadRequest($"Failed to send response: {Errors}");
        }
    }

    public async Task SendToSAP(List<SLBatchRequest> batchRequests)
    {
        HttpResponseMessage[] batchResult = await _slConnection.PostBatchAsync(batchRequests);

        foreach (var response in batchResult)
        {
            if (!response.IsSuccessStatusCode)
            {
                Errors++;
            }
        }
    }

}