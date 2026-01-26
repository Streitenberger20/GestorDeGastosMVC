namespace GestorDeGastos.Models
{
    public class Detalle
    {
        public int Id { get; set; }
        public string NombreDetalle { get; set; }

        public int RubroId { get; set; }
        public Rubro Rubro { get; set; }

        public bool esActivo { get; set; } = true;

        public Detalle() { }
    }
}

