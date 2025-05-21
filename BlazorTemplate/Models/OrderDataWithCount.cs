using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Models
{
    public class OrderDataWithCount
    {
        public int DocEntry { get; set; }
        public string CardCode { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public int IsProcessed { get; set; }
        public int HasError { get; set; }
        public string ErrorMessage { get; set; }
        public int LineCount { get; set; }
    }
}
