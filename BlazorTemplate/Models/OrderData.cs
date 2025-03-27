using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Models
{
    public class OrderData
    {
        public int ID { get; set; }
        public string Client { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();

    }
}
