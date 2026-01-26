using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestorDeGastos.ViewModels
{
    public class EditarRolViewModel
    {
        public int RolId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        public string NombreRol { get; set; }

        // Rubros disponibles en la app
        public List<SelectListItem> RubrosDisponibles { get; set; } = new();

        // IDs de rubros marcados (activos)
        [MinLength(1, ErrorMessage = "Debe seleccionar al menos un rubro.")]
        public List<int> RubrosSeleccionados { get; set; } = new();
    }
}
