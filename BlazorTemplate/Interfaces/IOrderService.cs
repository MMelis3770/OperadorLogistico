using BlazorTemplate.Models;
namespace BlazorTemplate.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderData>> GetOrdersAsync();
        Task<bool> ConfirmOrderToSQL(int orderId);
    }
}