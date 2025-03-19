using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperadorLogistico.Models
{
    public class Product
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }

        //SAP DATA
        public string SAPCode { get; set; }
        public string Category { get; set; }
    }
}
