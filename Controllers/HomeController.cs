using GestorDeGastos.Data;
using GestorDeGastos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace GestorDeGastos.Controllers
{
    [Authorize(Roles = "JEFE")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var seisMesesAtras = hoy.AddMonths(-5);

            var gastos = await _context.Gastos.Where(g => g.esActivo && g.Importe >= 0)
                .Include(g => g.Usuario)
                .Include(g => g.Rubro)
                .Include(g => g.Detalle)
                .ToListAsync();

            // 1. Filtrado por mes actual
            var gastosMes = gastos.Where(g => g.FechaGasto >= primerDiaMesActual).ToList();
            var gastosPesos = gastosMes.Where(g => g.Moneda == "AR$").ToList();
            var gastosDolares = gastosMes.Where(g => g.Moneda == "USD").ToList();

            // 2. Totales
            var totalMesPesos = gastosPesos.Sum(g => g.Importe);
            var totalMesDolares = gastosDolares.Sum(g => g.Importe);

            // 3. Gastos por rubro (del mes)
            var categoriasDelMes = gastosMes
                .GroupBy(g => g.Rubro.NombreRubro)
                .Select(gr => new CategoriaGastoViewModel
                {
                    Nombre = gr.Key,
                    TotalPesos = gr.Where(f => f.Moneda == "AR$").Sum(g => g.Importe),
                    TotalDolares = gr.Where(f => f.Moneda == "USD").Sum(g => g.Importe)
                }).ToList();

            // 4. Tablas de detalle
            var detallePesos = gastosPesos.Select(g => new GastoDetalleViewModel
            {
                Usuario = g.Usuario.NombreUsuario,
                Rubro = g.Rubro.NombreRubro,
                Detalle = g.Detalle.NombreDetalle,
                Monto = g.Importe,
                Fecha = g.FechaGasto
            }).ToList();

            var detalleDolares = gastosDolares.Select(g => new GastoDetalleViewModel
            {
                Usuario = g.Usuario.NombreUsuario,
                Rubro = g.Rubro.NombreRubro,
                Detalle = g.Detalle.NombreDetalle,
                Monto = g.Importe,
                Fecha = g.FechaGasto
            }).ToList();

            // 5. Comparativa últimos 6 meses
            var gastosUltimos6Meses = gastos.Where(g => g.FechaGasto >= new DateTime(seisMesesAtras.Year, seisMesesAtras.Month, 1));
            var comparativa = gastosUltimos6Meses
                .GroupBy(g => new { g.FechaGasto.Year, g.FechaGasto.Month })
                .OrderBy(gr => gr.Key.Year).ThenBy(gr => gr.Key.Month)
                .Select(gr => new ComparativaMensualViewModel
                {
                    Mes = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(gr.Key.Month)} {gr.Key.Year}",
                    TotalPesos = gr.Where(g => g.Moneda == "AR$").Sum(g => g.Importe),
                    TotalDolares = gr.Where(g => g.Moneda == "USD").Sum(g => g.Importe)
                }).ToList();

            var vm = new DashboardViewModel
            {
                TotalMesPesos = totalMesPesos,
                TotalMesDolares = totalMesDolares,
                CategoriasDelMes = categoriasDelMes,
                GastosPesos = detallePesos,
                GastosDolares = detalleDolares,
                Comparativa6Meses = comparativa
            };

            return View(vm);
        }

    }
}
