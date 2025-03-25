using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OperadorLogistico.Console.Communication.Api.Dto;
using OperadorLogistico.Console.Models;

namespace OperadorLogistico.Console.Communication.Api
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfig _config;

        public ApiClient(ApiConfig config)
        {
            _config = config;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);
        }

        public async Task<bool> SendConfirmationAsync(Order order)
        {
            try
            {
                var confirmationDto = MapToConfirmationDto(order);
                var json = JsonSerializer.Serialize(confirmationDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/confirmations", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Error al enviar confirmación: {response.StatusCode}, {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error en la comunicación con la API: {ex.Message}");
                return false;
            }
        }

        private ConfirmationDto MapToConfirmationDto(Order order)
        {
            var confirmationDto = new ConfirmationDto
            {
                OrderNumber = order.OrderNumber,
                ConfirmationDate = DateTime.Now,
                Status = order.Status.ToString()
            };

            foreach (var line in order.Lines)
            {
                var lineDto = new ConfirmationLineDto
                {
                    LineNumber = line.LineNumber,
                    ProductCode = line.ProductCode,
                    RequestedQuantity = line.RequestedQuantity,
                    //AssignedQuantity = line.AssignedQuantity,
                    Status = line.Status.ToString()
                };

                foreach (var batch in line.AssignedBatches)
                {
                    lineDto.Batches.Add(new BatchAssignmentDto
                    {
                        BatchCode = batch.Key,
                        Quantity = batch.Value
                    });
                }

                confirmationDto.Lines.Add(lineDto);
            }

            return confirmationDto;
        }
    }
}
