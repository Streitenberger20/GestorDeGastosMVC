using System.ComponentModel.DataAnnotations;

namespace GestorDeGastos.ViewModels
{
    public class EditarDetalleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción del detalle es obligatoria.")]
        public string Descripcion { get; set; }
    }
}
