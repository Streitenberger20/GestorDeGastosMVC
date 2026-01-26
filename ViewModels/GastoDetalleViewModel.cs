using DocumentFormat.OpenXml.Office2010.PowerPoint;

namespace GestorDeGastos.ViewModels
{
    public class GastoDetalleViewModel
    {

        public int NumeroGasto { get; set; }

        public bool EsActivo { get; set; }

        public string Usuario { get; set; }
        public string Rubro { get; set; }
        public string Detalle { get; set; } 
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }

        public string? Comentario { get; set; }

        public string Moneda { get; set; }
    }
}
