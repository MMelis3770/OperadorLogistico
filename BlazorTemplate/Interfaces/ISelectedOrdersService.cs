using BlazorTemplate.Models;

namespace BlazorTemplate.Interfaces
{
    public interface ISelectedOrdersService
    {
        List<OrderData> SelectedOrders { get; }
        void SetSelectedOrders(List<OrderData> orders);
        void ClearSelectedOrders();
    }
}
