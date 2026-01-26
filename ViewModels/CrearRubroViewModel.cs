using System.ComponentModel.DataAnnotations;

namespace GestorDeGastos.ViewModels
{
    public class CrearRubroViewModel
    {

        [Required(ErrorMessage = "El nombre del rubro es obligatorio.")]
        public string NombreRubro { get; set; }

        [MinLength(1, ErrorMessage = "Debe agregar al menos un detalle.")]
        public List<string> DetallesDescripcion { get; set; } = new List<string>();

        [MinLength(1, ErrorMessage = "Debe seleccionar al menos un rol.")]
        public List<int> RolesSeleccionados { get; set; } = new List<int>();

    }
}
