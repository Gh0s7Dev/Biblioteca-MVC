
using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        // Cualquier usuario autenticado puede consultar.
        public IActionResult Index()
        {
            var libros = _context.Libros.ToList();
            return View(libros);
        }

        public IActionResult Details(int id)
        {
            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Solo Administrador puede crear.
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            _context.Libros.Add(libro);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Solo Administrador puede editar.
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Edit(int id)
        {
            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Edit(int id, Libro libro)
        {
            if (id != libro.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            _context.Libros.Update(libro);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Solo Administrador puede acceder a la confirmación.
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Delete(int id)
        {
            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // También se protege la operación POST de eliminación.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult DeleteConfirmed(int id)
        {
            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            _context.Libros.Remove(libro);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Libro eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
