
using GestorDeGastos.Data;
using GestorDeGastos.Models;
using GestorDeGastos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorDeGastos.Controllers
{
    [Authorize]
    public class GastosController : Controller
    {
        private readonly AppDbContext _context;

        public GastosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ListadoGastos()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var gastos = _context.Gastos
                .Where(g => g.UsuarioId == usuarioId && g.esActivo && g.Importe > 0 && g.FechaGasto.Month == (DateTime.Now).Month && g.FechaGasto.Year == DateTime.Now.Year)
                .Include(g => g.Rubro)
                .Include(g => g.Detalle)
                .OrderByDescending(g => g.FechaGasto)
                .ThenByDescending(g => g.Id)
                .ToList();
            return View(gastos);
        }

        [HttpGet]
        public IActionResult CrearGasto()
        {
            var rolNombre = User.FindFirst(ClaimTypes.Role)?.Value;

            var rol = _context.Roles
                .Include(r => r.RolRubros.Where(rr => rr.EsActivo))
                    .ThenInclude(rr => rr.Rubro)
                    .ThenInclude(r => r.Detalles.Where(d => d.esActivo))
                .FirstOrDefault(r => r.NombreRol == rolNombre);

            var rubros = rol?.RolRubros
                .Select(rr => rr.Rubro)
                .Where(rr => rr.Detalles.Count != 0 )
                .OrderBy(r => r.NombreRubro)
                .ToList() ?? new List<Rubro>();


            var detalles = _context.Detalles
                .Where(d => d.esActivo)
                .OrderBy(d => d.NombreDetalle)
                .ToList();

            ViewBag.Rubros = new SelectList(rubros, "Id", "NombreRubro");


            var gastoVM = new GastoViewModel();

            return View(gastoVM);
        }

        [HttpGet]
        public JsonResult ObtenerDetalles(int rubroId)
        {
            var detalles = _context.Detalles
                .Where(d => d.RubroId == rubroId && d.esActivo)
                .Select(d => new { d.Id, d.NombreDetalle})
                .OrderBy(d => d.NombreDetalle)
                .ToList();

            return Json(detalles);
        }

        [HttpPost]
        public IActionResult CrearGasto(GastoViewModel gastoVM)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!ModelState.IsValid)
            {

                var rolNombre = User.FindFirst(ClaimTypes.Role)?.Value;

                var rol = _context.Roles
                    .Include(r => r.RolRubros.Where(rr => rr.EsActivo))
                        .ThenInclude(rr => rr.Rubro)
                        .ThenInclude(r => r.Detalles.Where(d => d.esActivo))
                    .FirstOrDefault(r => r.NombreRol == rolNombre);

                var rubros = rol?.RolRubros
                    .Select(rr => rr.Rubro)
                    .Where(rr => rr.Detalles.Count != 0)
                    .OrderBy(r => r.NombreRubro)
                    .ToList() ?? new List<Rubro>();


                var detalles = _context.Detalles
                    .Where(d => d.esActivo)
                    .OrderBy(d => d.NombreDetalle)
                    .ToList();

                ViewBag.Rubros = new SelectList(rubros, "Id", "NombreRubro");

                return View(gastoVM);
            }
            var gasto = new Gasto
            {
                UsuarioId = usuarioId,
                FechaGasto = gastoVM.FechaGasto,
                Importe = gastoVM.Importe.Value,
                Moneda = gastoVM.Moneda,
                DetalleId = gastoVM.DetalleId,
                RubroId = gastoVM.RubroId,
                Comentario = gastoVM.Comentario,
            };

            _context.Gastos.Add(gasto);
            _context.SaveChanges();

            return RedirectToAction("ListadoGastos");
        }

        [HttpGet]
        public IActionResult EditarGasto(int id)
        {
            var gasto = _context.Gastos.Find(id);
            if (gasto == null) return NotFound();
            var rolNombre = User.FindFirst(ClaimTypes.Role)?.Value;
            var rol = _context.Roles
                .Include(r => r.RolRubros.Where(rr => rr.EsActivo))
                    .ThenInclude(rr => rr.Rubro)
                    .ThenInclude(r => r.Detalles.Where(d => d.esActivo))
                .FirstOrDefault(r => r.NombreRol == rolNombre);
            var rubros = rol?.RolRubros
                .Select(rr => rr.Rubro)
                .Where(rr => rr.Detalles.Count != 0)
                .OrderBy(r => r.NombreRubro)
                .ToList() ?? new List<Rubro>();
            var detalles = _context.Detalles
                .Where(d => d.esActivo)
                .OrderBy(d => d.NombreDetalle)
                .ToList();
            ViewBag.Rubros = new SelectList(rubros, "Id", "NombreRubro", gasto.RubroId);
            ViewBag.Detalles = new SelectList(detalles, "Id", "NombreDetalle", gasto.DetalleId);
            ViewBag.RubroNombre = gasto.Rubro?.NombreRubro ?? "";
            ViewBag.DetalleNombre = gasto.Detalle?.NombreDetalle ?? "";
            ViewBag.GastoId = id;
            var gastoVM = new GastoViewModel
            {
                FechaGasto = gasto.FechaGasto,
                Importe = gasto.Importe,
                Moneda = gasto.Moneda,
                DetalleId = gasto.DetalleId,
                RubroId = gasto.RubroId,
                Comentario = gasto.Comentario,
            };
            return View(gastoVM);
        }

        [HttpPost]
        public IActionResult EditarGasto(int id, GastoViewModel gastoVM)
        {
            if (!ModelState.IsValid)
            {
                var rolNombre = User.FindFirst(ClaimTypes.Role)?.Value;
                var rol = _context.Roles
                    .Include(r => r.RolRubros.Where(rr => rr.EsActivo))
                        .ThenInclude(rr => rr.Rubro)
                        .ThenInclude(r => r.Detalles.Where(d => d.esActivo))
                    .FirstOrDefault(r => r.NombreRol == rolNombre);
                var rubros = rol?.RolRubros
                    .Select(rr => rr.Rubro)
                    .Where(rr => rr.Detalles.Count != 0)
                    .OrderBy(r => r.NombreRubro)
                    .ToList() ?? new List<Rubro>();
                var detalles = _context.Detalles
                    .Where(d => d.esActivo)
                    .OrderBy(d => d.NombreDetalle)
                    .ToList();
                ViewBag.Rubros = new SelectList(rubros, "Id", "NombreRubro", gastoVM.RubroId);
                ViewBag.Detalles = new SelectList(detalles, "Id", "NombreDetalle", gastoVM.DetalleId);
                return View(gastoVM);
            }
            var gasto = _context.Gastos.Find(id);

            gasto.FechaGasto = gastoVM.FechaGasto;
            gasto.Importe = gastoVM.Importe.Value;
            gasto.Moneda = gastoVM.Moneda;
            gasto.DetalleId = gastoVM.DetalleId;
            gasto.RubroId = gastoVM.RubroId;
            gasto.Comentario = gastoVM.Comentario;
            _context.SaveChanges();
            return RedirectToAction("ListadoGastos");
        }

        [HttpPost]
        public IActionResult EliminarGasto(int id)
        {
            var gasto = _context.Gastos.Find(id);
            if (gasto == null) return NotFound();
            gasto.esActivo = false;
            gasto.Comentario = "Gasto eliminado: " + gasto.Id;

            var credito = new Gasto
            {
                UsuarioId = gasto.UsuarioId,
                FechaGasto = gasto.FechaGasto,
                Importe = -gasto.Importe,
                Moneda = gasto.Moneda,
                DetalleId = gasto.DetalleId,
                RubroId = gasto.RubroId,
                Comentario = "Credito eliminación de gasto: " + gasto.Id,
            };

            _context.Gastos.Add(credito);

            _context.SaveChanges();
            return RedirectToAction("ListadoGastos");
        }
    }
}
