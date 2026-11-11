using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using BibliotecaMVC.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<AutoresController> _logger;

        public AutoresController(BibliotecaContext context, ILogger<AutoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Consultando el listado de autores");

            try
            {
                var autores = _context.Autores.ToList();
                return View(autores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al consultar el listado de autores.");
                throw;   
            }
        }

        public IActionResult Details(int id)
        {
            _logger.LogInformation("Consultando detalles de un autor con identificador {AutorId}", id);
            var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

            try
            {
                if (autor == null)
                {
                    _logger.LogWarning("No se encontró un autor con identificador {AutorId}", id);
                    return NotFound();
                }

                return View(autor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al consultar los detalles del autor con identificador {AutorId}.", id);
                throw;
            }
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operación de creación de un autor");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Create(Autor autor)
        {
            _logger.LogInformation("Iniciando la operación de creación de un autor");
            if (!ModelState.IsValid)
            {
                _logger.LogError("El autor enviado no cumple con los requerimientos mínimos");
                return View(autor);
            }

            try
            {
                _context.Autores.Add(autor);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al crear el autor.");
                throw;
            }
        }
        
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Edit(int id)
        {
            _logger.LogInformation("Iniciando operación de edición de un autor con identificador {AutorId}", id);
            try
            {
                var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

                if (autor == null)
                {
                    return NotFound();
                }

                return View(autor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al consultar los detalles del autor para su edición con identificador {AutorId}.", id);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id, Autor autor)
        {
            _logger.LogInformation("Iniciando la edición del autor con identificador {AutorId}", id);
            if (id != autor.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            try
            {
                var exist = await _context.Autores.AnyAsync(a => a.ID == id);
                if(!exist)
                {
                    _logger.LogWarning("No se encontró un autor con identificador {AutorId} para editar", id);
                    return NotFound();
                }

                _context.Update(autor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al editar el autor con identificador {AutorId}.", id);
                throw;
            }
        }

        // POST: Autores/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Iniciando la eliminación del autor con identificador {AutorId}", id);
            try
            {
                var autor = await _context.Autores.FindAsync(id);

                if (autor == null)
                {
                    _logger.LogWarning("No se encontró un autor con identificador {AutorId} para eliminar", id);
                    return NotFound();
                }

                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al eliminar el autor con identificador {AutorId}.", id);
                throw;
            }
        }
    }
}