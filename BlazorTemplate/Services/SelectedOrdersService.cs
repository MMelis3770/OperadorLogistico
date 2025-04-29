using BlazorTemplate.Interfaces;
using BlazorTemplate.Models;

namespace BlazorTemplate.Services
{
    public class SelectedOrdersService : ISelectedOrdersService
    {
        private List<OrderData> _selectedOrders = new List<OrderData>();

        public List<OrderData> SelectedOrders => _selectedOrders;

        public void SetSelectedOrders(List<OrderData> orders)
        {
            _selectedOrders = orders ?? new List<OrderData>();
        }

        public void ClearSelectedOrders()
        {
            _selectedOrders.Clear();
        }
    }
}
