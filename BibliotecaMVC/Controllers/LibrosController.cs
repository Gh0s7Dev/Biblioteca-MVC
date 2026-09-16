using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly IRepositorioLibro _repositorio;

        public LibrosController(IRepositorioLibro repositorio)
        {
            _repositorio = repositorio;
        }
        

        // Listar libros
        public IActionResult Index()
        {
            var libros = _repositorio.ObtenerTodos();
            return View(libros);
        }

        // Ver detalle
        public IActionResult Details(int id)
        {
            var libros = _repositorio.ObtenerTodos();
            var libro = libros.FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Mostrar formulario para crear
        public IActionResult Create()
        {
            return View();
        }

        // Crear libro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            var libros = _repositorio.ObtenerTodos();

            libro.ID = libros.Any()
                ? libros.Max(l => l.ID) + 1
                : 1;

            _repositorio.Agregar(libro);

            return RedirectToAction(nameof(Index));
        }

        // Mostrar formulario para editar
        public IActionResult Edit(int id)
        {
            var libros = _repositorio.ObtenerTodos();
            var libro = libros.FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Editar libro
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            var libros = _repositorio.ObtenerTodos();
            var libroExistente = libros.FirstOrDefault(l => l.ID == id);

            if (libroExistente == null)
            {
                return NotFound();
            }

            libroExistente.Titulo = libro.Titulo;
            libroExistente.Autor = libro.Autor;
            libroExistente.Categoria = libro.Categoria;
            libroExistente.Precio = libro.Precio;
            libroExistente.Disponible = libro.Disponible;

            return RedirectToAction(nameof(Index));
        }

        // Confirmar eliminación
        public IActionResult Delete(int id)
        {
            var libros = _repositorio.ObtenerTodos();
            var libro = libros.FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Eliminar libro
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var libros = _repositorio.ObtenerTodos();
            var libro = libros.FirstOrDefault(l => l.ID == id);

            if (libro == null)
            {
                return NotFound();
            }

            _repositorio.Eliminar(libro);

            return RedirectToAction(nameof(Index));
        }
    }
}