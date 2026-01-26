using GestorDeGastos.Models;
using System.ComponentModel.DataAnnotations;

namespace GestorDeGastos.ViewModels
{
    public class EditarRubroViewModel
    {



        [Required(ErrorMessage = "El ID del rubro es obligatorio.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del rubro es obligatorio.")]
        public string NombreRubro { get; set; }

        [MinLength(1, ErrorMessage = "Debe agregar al menos un detalle.")]
        public List<EditarDetalleViewModel> Detalles { get; set; } = new List<EditarDetalleViewModel>();

        [MinLength(1, ErrorMessage = "Debe seleccionar al menos un rol.")]
        public List<int> RolesSeleccionados { get; set; } = new List<int>();

        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();
    }
}
