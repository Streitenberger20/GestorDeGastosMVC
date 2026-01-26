using GestorDeGastos.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorDeGastos.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext db;

        public LoginController(AppDbContext context)
        {
            db = context;
        }


        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("JEFE"))
            {
               return RedirectToAction("Index", "Home");
            }
            else if (User.Identity.IsAuthenticated && !User.IsInRole("JEFE"))
            {
                return RedirectToAction("ListadoGastos", "Gastos");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string nombreUsuario, string contraseña)
        {
            nombreUsuario = nombreUsuario.ToUpper().Trim();

            var usuario = db.Usuarios.Include(u => u.Rol)
                .FirstOrDefault(u => u.NombreUsuario.ToUpper() == nombreUsuario && u.Contraseña == contraseña && u.esActivo);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, usuario.Rol.NombreRol)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (principal.IsInRole("JEFE"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("ListadoGastos", "Gastos");
                }
            }

            ViewBag.Error = "Usuario o contraseña incorrecta";
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
