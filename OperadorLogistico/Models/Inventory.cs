using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperadorLogistico.Models
{
    public class Inventory
    {
        public string CodigoProducto { get; set; }
        public Producto Producto { get; set; }
        public decimal CantidadDisponible { get; set; }
        public ICollection<Lote> Lotes { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public string Almacen { get; set; }
    }
}
