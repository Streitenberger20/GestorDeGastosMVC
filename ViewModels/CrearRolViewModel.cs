using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestorDeGastos.ViewModels
{
    public class CrearRolViewModel
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        public string NombreRol { get; set; }

        [MinLength(1, ErrorMessage = "Debe seleccionar al menos un rubro.")]
        public List<int> RubrosSeleccionados { get; set; } = new();
        public List<SelectListItem> RubrosDisponibles { get; set; } = new();
    }
}
