using ClosedXML.Excel;
using GestorDeGastos.Data;
using GestorDeGastos.Models;
using GestorDeGastos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorDeGastos.Controllers
{
    [Authorize(Roles = "JEFE")]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListadoGastos(DateTime? fechaDesde, DateTime? fechaHasta, int? usuarioId, int? rubroId, string moneda)
        {


            var query = _context.Gastos
                .Include(g => g.Usuario)
                .Include(g => g.Rubro)
                .Include(g => g.Detalle)
                .AsQueryable();

            // Aplicar filtros
            query = query.Where(g => g.FechaGasto >= fechaDesde && g.FechaGasto <= fechaHasta && g.Moneda == moneda);

            if (usuarioId.HasValue)
                query = query.Where(g => g.UsuarioId == usuarioId.Value);

            if (rubroId.HasValue)
                query = query.Where(g => g.RubroId == rubroId.Value);

            var resultados = await query.OrderByDescending(g => g.FechaGasto)
                .Select(g => new GastoDetalleViewModel
                {
                    NumeroGasto = g.Id,
                    EsActivo = g.esActivo,
                    Fecha = g.FechaGasto,
                    Usuario = g.Usuario.NombreUsuario,
                    Rubro = g.Rubro.NombreRubro,
                    Detalle = g.Detalle.NombreDetalle,
                    Monto = g.Importe,
                    Moneda = g.Moneda,
                    Comentario = g.Comentario
                })
                .ToListAsync();

            var vm = new GastoFiltroViewModel
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                UsuarioId = usuarioId,
                RubroId = rubroId,
                Usuarios = await _context.Usuarios
                .Where(u => u.esActivo)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync(),
                Rubros = await _context.Rubros
                .Where(r => r.esActivo)
                .OrderBy(r => r.NombreRubro)
                .ToListAsync(),
                Resultados = resultados,
                Total = resultados.Sum(r => r.Monto),
                Moneda = moneda
            };

            return View(vm);
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ExportarExcel(DateTime? fechaDesde, DateTime? fechaHasta, int? usuarioId, int? rubroId, string moneda)
        {
            var query = _context.Gastos
                .Include(g => g.Usuario)
                .Include(g => g.Rubro)
                .Include(g => g.Detalle)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(g => g.FechaGasto >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(g => g.FechaGasto <= fechaHasta.Value);

            if (usuarioId.HasValue)
                query = query.Where(g => g.UsuarioId == usuarioId.Value);

            if (rubroId.HasValue)
                query = query.Where(g => g.RubroId == rubroId.Value);

            if (!string.IsNullOrEmpty(moneda))
                query = query.Where(g => g.Moneda == moneda);

            var lista = await query
                .OrderByDescending(g => g.FechaGasto)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Gastos");

            int fila = 1;

            // ================== BLOQUE 1: Gastos normales ==================
            ws.Cell(fila, 1).Value = "📌 Gastos Activos";
            ws.Range(fila, 1, fila, 7).Merge().Style
                .Font.SetBold()
                .Font.FontColor = XLColor.White;
            ws.Range(fila, 1, fila, 7).Style.Fill.BackgroundColor = XLColor.Green;
            fila++;

            EscribirEncabezados(ws, fila); fila++;

            foreach (var g in lista.Where(x => x.esActivo && x.Importe >= 0))
            {
                EscribirFila(ws, g, fila++);
            }

            // Total bloque 1
            var totalActivos = lista.Where(x => x.esActivo && x.Importe >= 0).Sum(x => x.Importe);
            ws.Cell(fila, 3).Value = "TOTAL";
            ws.Cell(fila, 4).Value = totalActivos;
            ws.Cell(fila, 3).Style.Font.Bold = true;
            ws.Cell(fila, 4).Style.Font.Bold = true;
            ws.Cell(fila, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Cell(fila, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Cell(fila, 4).Style.NumberFormat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            fila += 2;

            // ================== BLOQUE 2: Cancelados y Créditos ==================
            ws.Cell(fila, 1).Value = "📌 Gastos Cancelados y Créditos";
            ws.Range(fila, 1, fila, 7).Merge().Style
                .Font.SetBold()
                .Font.FontColor = XLColor.Black;
            ws.Range(fila, 1, fila, 7).Style.Fill.BackgroundColor = XLColor.Orange;
            fila++;

            EscribirEncabezados(ws, fila); fila++;

            foreach (var g in lista.Where(x => !x.esActivo || x.Importe < 0))
            {
                if (!g.esActivo)
                    EscribirFila(ws, g, fila++, XLColor.LightPink); // cancelado
                else if (g.Importe < 0)
                    EscribirFila(ws, g, fila++, XLColor.LightYellow); // crédito
            }

            var totalEspeciales = lista.Where(x => !x.esActivo || x.Importe < 0).Sum(x => x.Importe);
            ws.Cell(fila, 3).Value = "TOTAL";
            ws.Cell(fila, 4).Value = totalEspeciales;
            ws.Cell(fila, 3).Style.Font.Bold = true;
            ws.Cell(fila, 4).Style.Font.Bold = true;
            ws.Cell(fila, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Cell(fila, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Cell(fila, 4).Style.NumberFormat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            fila += 2;

            // ================== TOTAL GENERAL ==================
            ws.Cell(fila, 5).Value = "TOTAL GENERAL";
            ws.Cell(fila, 6).Value = lista.Sum(x => x.Importe);
            ws.Range(fila, 5, fila, 6).Style.Font.Bold = true;
            ws.Cell(fila, 6).Style.NumberFormat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            ws.Cell(fila, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Cell(fila, 6).Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Columns().AdjustToContents();

            // Devolver archivo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReportesGastos-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        private void EscribirEncabezados(IXLWorksheet ws, int fila)
        {
            ws.Cell(fila, 1).Value = "Fecha";
            ws.Cell(fila, 2).Value = "Usuario";
            ws.Cell(fila, 3).Value = "Moneda";
            ws.Cell(fila, 4).Value = "Monto";
            ws.Cell(fila, 5).Value = "Rubro";
            ws.Cell(fila, 6).Value = "Detalle";
            ws.Cell(fila, 7).Value = "Comentario";

            ws.Cell(fila, 4).Style.NumberFormat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";

            ws.Range(fila, 1, fila, 7).Style.Font.Bold = true;
            ws.Range(fila, 1, fila, 7).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        private void EscribirFila(IXLWorksheet ws, Gasto g, int fila, XLColor? color = null)
        {
            ws.Cell(fila, 1).Value = g.FechaGasto.ToString("yyyy-MM-dd");
            ws.Cell(fila, 2).Value = g.Usuario.NombreUsuario;
            ws.Cell(fila, 3).Value = g.Moneda;
            ws.Cell(fila, 4).Value = g.Importe;
            ws.Cell(fila, 5).Value = g.Rubro.NombreRubro;
            ws.Cell(fila, 6).Value = g.Detalle.NombreDetalle;
            ws.Cell(fila, 7).Value = g.Comentario;

            ws.Cell(fila, 4).Style.NumberFormat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";

            if (color != null)
            {
                ws.Range(fila, 1, fila, 7).Style.Fill.BackgroundColor = color;
            }
        }
    }
}
