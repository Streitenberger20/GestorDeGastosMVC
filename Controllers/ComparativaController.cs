using GestorDeGastos.Data;
using GestorDeGastos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GestorDeGastos.Controllers
{
    [Authorize(Roles = "JEFE")]
    public class ComparativaController : Controller
    {
        private readonly AppDbContext _context;

        public ComparativaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ObtenerDatos([FromBody] List<MesAnioFiltro> filtros)
        {
            var datos = new List<ComparativaRubroMes>();

            foreach (var filtro in filtros)
            {
                var gastos = _context.Gastos
                    .Include(g => g.Rubro)
                    .Where(g => g.FechaGasto.Month == filtro.Mes && g.FechaGasto.Year == filtro.Anio)
                    .ToList();

                if (!gastos.Any()) continue;

                var totalPorMoneda = gastos
                    .GroupBy(g => g.Moneda)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Importe));

                var agrupados = gastos
                    .GroupBy(g => new { g.Rubro.NombreRubro, g.Moneda })
                    .Select(g => new ComparativaRubroMes
                    {
                        Rubro = g.Key.NombreRubro,
                        Monto = g.Sum(x => x.Importe),
                        Moneda = g.Key.Moneda,
                        Mes = filtro.Mes,
                        Anio = filtro.Anio,
                        Porcentaje = totalPorMoneda.ContainsKey(g.Key.Moneda) && totalPorMoneda[g.Key.Moneda] > 0
                            ? (g.Sum(x => x.Importe) / totalPorMoneda[g.Key.Moneda]) * 100
                            : 0
                    })
                    .ToList();

                datos.AddRange(agrupados);
            }

            return Json(datos);
        }
    }
}