using GestorDeGastos.Models;

namespace GestorDeGastos.ViewModels
{
    public class GastoFiltroViewModel
    {
        // Filtros
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? UsuarioId { get; set; }
        public int? RubroId { get; set; }

        // Listas para Select
        public List<Usuario> Usuarios { get; set; } = new();
        public List<Rubro> Rubros { get; set; } = new();

        public String Moneda { get; set; }
        public List<String> Monedas { get; set; } = new()
        {
            "AR$", "USD"
        };

        public decimal Total { get; set; }


        // Resultado
        public List<GastoDetalleViewModel> Resultados { get; set; } = new();
    }
}
