using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services
{
    public interface IAutorService
    {
        IEnumerable<Autor> ObtenerTodos();
        Autor ObtenerPorId(int id);

        Autor Agregar(Autor autor);
        bool Actualizar(Autor autor);
        bool Eliminar(int id);
    }
}