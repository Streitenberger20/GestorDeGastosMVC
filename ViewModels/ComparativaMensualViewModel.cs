namespace GestorDeGastos.ViewModels
{
    public class ComparativaMensualViewModel
    {
        public string Mes { get; set; } 
        public decimal TotalPesos { get; set; }
        public decimal TotalDolares { get; set; }
    }

    public class MesAnioFiltro
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
    }

    public class ComparativaRubroMes
    {
        public string Rubro { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
        public string Moneda { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
    }

    public class ComparativaMesesViewModel
    {
        public List<MesAnioFiltro> Filtros { get; set; } = new List<MesAnioFiltro>();
        public List<ComparativaRubroMes> Datos { get; set; } = new List<ComparativaRubroMes>();
    }


}
