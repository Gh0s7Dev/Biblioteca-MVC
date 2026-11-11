
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
        private readonly ILogger<LibrosController> _logger;

        public LibrosController(
            BibliotecaContext context,
            ILogger<LibrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Cualquier usuario autenticado puede consultar.
        public IActionResult Index()
        {
            _logger.LogInformation("Iniciando consulta de todos los libros.");

            var libros = _context.Libros.ToList();

            _logger.LogInformation(
                "Consulta de libros finalizada. Registros encontrados: {Cantidad}",
                libros.Count);

            return View(libros);
        }

        public IActionResult Details(int id)
        {
            _logger.LogInformation(
                "Iniciando consulta de detalles del libro ID: {LibroId}",
                id);

            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                _logger.LogWarning(
                    "No se encontró el libro con ID: {LibroId}",
                    id);

                return NotFound();
            }

            return View(libro);
        }

        // Solo Administrador y Bibliotecario pueden crear.
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Create()
        {
            _logger.LogInformation("Acceso al formulario de creación de libros.");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Create(Libro libro)
        {
            _logger.LogInformation("Iniciando operación de creación de libro.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "No se pudo crear el libro porque los datos proporcionados no son válidos.");

                return View(libro);
            }

            try
            {
                _context.Libros.Add(libro);
                _context.SaveChanges();

                _logger.LogInformation(
                    "Libro creado correctamente. ID: {LibroId}",
                    libro.ID);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al crear un libro.");

                throw;
            }
        }

        // Solo Administrador y Bibliotecario pueden editar.
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Edit(int id)
        {
            _logger.LogInformation(
                "Iniciando consulta para editar el libro ID: {LibroId}",
                id);

            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                _logger.LogWarning(
                    "No se encontró el libro ID: {LibroId} para editar.",
                    id);

                return NotFound();
            }

            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Edit(int id, Libro libro)
        {
            _logger.LogInformation(
                "Iniciando operación de edición del libro ID: {LibroId}",
                id);

            if (id != libro.ID)
            {
                _logger.LogWarning(
                    "El ID de la URL no coincide con el ID del libro enviado.");

                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "No se pudo editar el libro ID: {LibroId} porque los datos no son válidos.",
                    id);

                return View(libro);
            }

            try
            {
                _context.Libros.Update(libro);
                _context.SaveChanges();

                _logger.LogInformation(
                    "Libro actualizado correctamente. ID: {LibroId}",
                    id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al editar el libro ID: {LibroId}",
                    id);

                throw;
            }
        }

        // Solo Administrador y Bibliotecario pueden acceder a la confirmación.
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation(
                "Iniciando consulta para eliminar el libro ID: {LibroId}",
                id);

            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                _logger.LogWarning(
                    "No se encontró el libro ID: {LibroId} para eliminar.",
                    id);

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
            _logger.LogInformation(
                "Iniciando eliminación del libro ID: {LibroId}",
                id);

            var libro = _context.Libros
                .FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                _logger.LogWarning(
                    "No se encontró el libro ID: {LibroId} para eliminar.",
                    id);

                return NotFound();
            }

            try
            {
                _context.Libros.Remove(libro);
                _context.SaveChanges();

                _logger.LogInformation(
                    "Libro eliminado correctamente. ID: {LibroId}",
                    id);

                TempData["SuccessMessage"] =
                    "Libro eliminado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar el libro ID: {LibroId}",
                    id);

                throw;
            }
        }
    }
}
