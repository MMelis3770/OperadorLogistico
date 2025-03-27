using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Models
{
    public class LineItem
    {
        public int OrderID { get; set; }
        public int LineNumber { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
    }
}
