using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorDeGastos.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Campo obligatorio")]
        public string NombreUsuario { get; set; }
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Contraseña { get; set; }

        public int RolId { get; set; }
        [ValidateNever]
        public Rol Rol { get; set; }
        [ValidateNever]
        public ICollection<Gasto> Gastos { get; set; }

        public bool esActivo { get; set; } = true;
        public Usuario() { }
    }
}
