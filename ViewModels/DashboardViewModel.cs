

namespace GestorDeGastos.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalMesPesos { get; set; }
        public decimal TotalMesDolares { get; set; }

        public List<CategoriaGastoViewModel> CategoriasDelMes { get; set; } = new();
        public List<GastoDetalleViewModel> GastosPesos { get; set; } = new();
        public List<GastoDetalleViewModel> GastosDolares { get; set; } = new();
        public List<ComparativaMensualViewModel> Comparativa6Meses { get; set; } = new();


    }
}
