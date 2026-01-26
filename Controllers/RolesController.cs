using DocumentFormat.OpenXml.Office2010.Excel;
using GestorDeGastos.Data;
using GestorDeGastos.Models;
using GestorDeGastos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace GestorDeGastos.Controllers
{
    [Authorize(Roles = "JEFE")]
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> ListadoRoles()
        {
            return View(await _context.Roles.Where(r => r.esActivo).ToListAsync());
        }

        public IActionResult CrearRol()
        {
            var rubros = _context.Rubros.Where(r => r.esActivo)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.NombreRubro
                }).ToList();

            var model = new CrearRolViewModel
            {
                RubrosDisponibles = rubros
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CrearRol(CrearRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RubrosDisponibles = _context.Rubros.Where(r => r.esActivo)
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.NombreRubro
                    }).ToList();
                return View(model);
            }

            // Normalizamos a MAYÚSCULAS
            model.NombreRol = model.NombreRol.ToUpper().Trim();

            // Validar que no exista ya
            var existeRol = _context.Roles.Where(r => r.esActivo)
                .Any(r => r.NombreRol.ToUpper() == model.NombreRol);

            if (existeRol)
            {
                ModelState.AddModelError("NombreRol", "Ya existe un rol con este nombre.");
                model.RubrosDisponibles = _context.Rubros.Where(r => r.esActivo)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.NombreRubro
                }).ToList();
                return View(model);
            }


            // Crear rol
            var nuevoRol = new Rol
            {
                NombreRol = model.NombreRol
            };
            _context.Roles.Add(nuevoRol);
            _context.SaveChanges();

            // Asignar rubros seleccionados
            if (model.RubrosSeleccionados != null)
            {
                foreach (var rubroId in model.RubrosSeleccionados)
                {
                    _context.RolRubros.Add(new RolRubro
                    {
                        RolId = nuevoRol.Id,
                        RubroId = rubroId
                    });
                }
                _context.SaveChanges();
            }

            return RedirectToAction("ListadoRoles");
        }

        public IActionResult EditarRol(int id)
        {
            var rol = _context.Roles.Find(id);
            if (rol == null) return NotFound();

            var rubros = _context.Rubros
                .Where(r => r.esActivo)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.NombreRubro
                })
                .ToList();

            var rubrosAsignadosActivos = _context.RolRubros
                .Where(rr => rr.RolId == id && rr.EsActivo)
                .Select(rr => rr.RubroId)
                .ToList();

            var model = new EditarRolViewModel
            {
                RolId = rol.Id,
                NombreRol = rol.NombreRol,
                RubrosDisponibles = rubros,
                RubrosSeleccionados = rubrosAsignadosActivos
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditarRol(EditarRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RubrosDisponibles = _context.Rubros
                    .Where(r => r.esActivo)
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.NombreRubro
                    }).ToList();
                return View(model);
            }

            model.NombreRol = model.NombreRol?.ToUpper().Trim();

            var existeRol = _context.Roles.Where(r => r.esActivo)
                .Any(r => r.NombreRol.ToUpper() == model.NombreRol && r.Id != model.RolId);

            if (existeRol)
            {
                ModelState.AddModelError("NombreRol", "Ya existe un rol con este nombre.");
                model.RubrosDisponibles = _context.Rubros
                .Where(r => r.esActivo)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.NombreRubro
                }).ToList();
                return View(model);
            }


            var rol = _context.Roles.Find(model.RolId);
            if (rol == null) return NotFound();

            rol.NombreRol = model.NombreRol;

            // Obtener todas las asignaciones existentes (activas e inactivas)
            var asignacionesExistentes = _context.RolRubros
                .Where(rr => rr.RolId == model.RolId)
                .ToList();

            // IDs de rubros enviados desde el formulario (seleccionados)
            var rubrosSeleccionados = model.RubrosSeleccionados ?? new List<int>();

            // Activar o crear asignaciones para rubros seleccionados
            foreach (var rubroId in rubrosSeleccionados)
            {
                var asignacion = asignacionesExistentes.FirstOrDefault(rr => rr.RubroId == rubroId);
                if (asignacion == null)
                {
                    // Crear nueva asignación activa
                    _context.RolRubros.Add(new RolRubro
                    {
                        RolId = model.RolId,
                        RubroId = rubroId,
                        EsActivo = true
                    });
                }
                else if (!asignacion.EsActivo)
                {
                    // Reactivar asignación existente
                    asignacion.EsActivo = true;
                    _context.RolRubros.Update(asignacion);
                }
                // Si ya está activo, no hacemos nada
            }

            // Desactivar asignaciones que ya no están seleccionadas
            foreach (var asignacion in asignacionesExistentes)
            {
                if (!rubrosSeleccionados.Contains(asignacion.RubroId) && asignacion.EsActivo)
                {
                    asignacion.EsActivo = false;
                    _context.RolRubros.Update(asignacion);
                }
            }

            _context.SaveChanges();

            return RedirectToAction("ListadoRoles");
        }

        private bool RolExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}
