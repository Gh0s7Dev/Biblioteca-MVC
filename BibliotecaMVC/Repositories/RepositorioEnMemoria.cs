using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories
{
    public class RepositorioEnMemoria : IRepositorioLibro
    {
        private readonly List<Libro> _libros = new List<Libro>
        {
            new Libro
            {
                ID = 1,
                Titulo = "Cien años de soledad",
                Autor = "Gabriel García Márquez",
                Categoria = "Novela",
                Precio = 25.00m,
                Disponible = true
            },
            new Libro
            {
                ID = 2,
                Titulo = "El principito",
                Autor = "Antoine de Saint-Exupéry",
                Categoria = "Literatura",
                Precio = 15.50m,
                Disponible = true
            },
            new Libro
            {
                ID = 3,
                Titulo = "1984",
                Autor = "George Orwell",
                Categoria = "Ciencia ficción",
                Precio = 20.00m,
                Disponible = false
            }
        };

        public IEnumerable<Libro> ObtenerTodos()
        {
            return _libros;
        }

        public Libro Agregar(Libro libro)
        {
            _libros.Add(libro);
            return libro;
        }

        public bool Eliminar(Libro libro)
        {
            return _libros.Remove(libro);
        }
    }
}