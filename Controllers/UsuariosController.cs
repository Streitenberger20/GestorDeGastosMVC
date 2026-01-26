using GestorDeGastos.Data;
using GestorDeGastos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace GestorDeGastos.Controllers
{
    [Authorize(Roles = "JEFE")]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> ListadoUsuarios()
        {
            var appDbContext = _context.Usuarios.Include(u => u.Rol).Where(u => u.esActivo);
            return View(await appDbContext.ToListAsync());
        }

       
        // GET: Usuarios/Create
        public IActionResult CrearUsuario()
        {
            ViewData["Rol"] = new SelectList(_context.Roles.Where(r => r.esActivo), "Id", "NombreRol");
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario([Bind("Id,NombreUsuario,Contraseña,RolId")] Usuario usuario)
        {

            var rolesValidos = _context.Roles.Where(r => r.esActivo).Select(r => r.Id).ToList();

            if (!rolesValidos.Contains(usuario.RolId))
            {
                ModelState.AddModelError("RolId", "Entrada Incorrecta");
            }

            if (!ModelState.IsValid) {
                ViewData["Rol"] = new SelectList(_context.Roles.Where(r => r.esActivo), "Id", "NombreRol");
                return View(usuario);
            }

            // Normalizamos a MAYÚSCULAS
            usuario.NombreUsuario = usuario.NombreUsuario.ToUpper().Trim();

            // Validar que no exista ya
            var existeUsuario = _context.Usuarios
                .Any(r => r.NombreUsuario.ToUpper() == usuario.NombreUsuario);

            if (existeUsuario)
            {
                ModelState.AddModelError("NombreUsuario", "Ya existe un usuario con este nombre.");
                ViewData["Rol"] = new SelectList(_context.Roles.Where(r => r.esActivo), "Id", "NombreRol");
                return View(usuario);
            }


            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListadoUsuarios));
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> EditarUsuario(int? id)
        {

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["RolId"] = new SelectList(_context.Roles.Where(r => r.esActivo), "Id", "NombreRol", usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(int id, [Bind("Id,NombreUsuario,Contraseña,RolId")] Usuario usuario)
        {
            var rolesValidos = _context.Roles.Where(r => r.esActivo).Select(r => r.Id).ToList();

            if (!rolesValidos.Contains(usuario.RolId))
            {
                ModelState.AddModelError("RolId", "Entrada Incorrecta");
            }

            if (!ModelState.IsValid)
            {
                ViewData["RolId"] = new SelectList(_context.Roles.Where(r => r.esActivo), "Id", "NombreRol");
                return View(usuario);
            }


            usuario.NombreUsuario = usuario.NombreUsuario.ToUpper().Trim();

            var existeUsuario = _context.Usuarios
                .Any(r => r.NombreUsuario.ToUpper() == usuario.NombreUsuario && r.Id != usuario.Id);

            if (existeUsuario)
            {
                ModelState.AddModelError("NombreUsuario", "Ya existe un usuario con este nombre.");
                ViewData["RolId"] = new SelectList(_context.Roles.Where(r => r.esActivo), "Id", "NombreRol");
                return View(usuario);
            }


            _context.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListadoUsuarios));

        }



        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.esActivo = false;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListadoUsuarios));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
