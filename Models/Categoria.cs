namespace GestorDeGastos.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public bool esActivo { get; set; } = true;

        public Categoria() { }
    }
}
