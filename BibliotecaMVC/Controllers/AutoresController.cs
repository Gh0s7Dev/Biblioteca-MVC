using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers
{
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

        // GET: Autores/Delete/
        public IActionResult Delete(int id)
        {
            var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        // POST: Autores/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

            if (autor == null)
            {
                return NotFound();
            }

            _context.Autores.Remove(autor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}