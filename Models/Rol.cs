namespace GestorDeGastos.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string NombreRol { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
        public ICollection<RolRubro> RolRubros { get; set; }


        public bool esActivo { get; set; } = true;

        public Rol() { }
    }
}
