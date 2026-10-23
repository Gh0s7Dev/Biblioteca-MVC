using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;

        public AutoresController(BibliotecaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var autores = _context.Autores.ToList();

            return View(autores);
        }

        public IActionResult Details(int id)
        {
            var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            _context.Autores.Add(autor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Autor autor)
        {
            if (id != autor.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            _context.Autores.Update(autor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // POST: Autores/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
            {
                return NotFound();
            }

            _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}