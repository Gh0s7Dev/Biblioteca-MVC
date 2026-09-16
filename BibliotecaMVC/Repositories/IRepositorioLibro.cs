using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories
{
    public interface IRepositorioLibro
    {
        IEnumerable<Libro> ObtenerTodos();

        Libro Agregar(Libro libro);

        bool Eliminar(Libro libro);
    }
}