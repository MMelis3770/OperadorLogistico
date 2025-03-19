using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperadorLogistico.Models
{
    public class Inventory
    {
        public string ProductCode { get; set; }
        public Product Product { get; set; }
        public decimal AvailableQuantity { get; set; }
        public string Warehouse { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
