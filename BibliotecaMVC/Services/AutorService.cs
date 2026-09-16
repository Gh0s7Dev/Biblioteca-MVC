using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services
{
    public class AutorService : IAutorService
    {
        private static readonly List<Autor> _autores = new List<Autor>
        {
            new Autor
            {
                ID = 1,
                Nombre = "Gabriel",
                Apellido = "García Márquez",
                Nacionalidad = "Colombiana",
                FechaNacimiento = new DateTime(1927, 3, 6),
                Activo = false
            },
            new Autor
            {
                ID = 2,
                Nombre = "Isabel",
                Apellido = "Allende",
                Nacionalidad = "Chilena",
                FechaNacimiento = new DateTime(1942, 8, 2),
                Activo = true
            },
            new Autor
            {
                ID = 3,
                Nombre = "J.K.",
                Apellido = "Rowling",
                Nacionalidad = "Británica",
                FechaNacimiento = new DateTime(1965, 7, 31),
                Activo = true
            },
            new Autor
            {
                ID = 4,
                Nombre = "Haruki",
                Apellido = "Murakami",
                Nacionalidad = "Japonesa",
                FechaNacimiento = new DateTime(1949, 1, 12),
                Activo = true
            },
            new Autor
            {
                ID = 5,
                Nombre = "Chinua",
                Apellido = "Achebe",
                Nacionalidad = "Nigeriana",
                FechaNacimiento = new DateTime(1930, 11, 16),
                Activo = false
            }
        };

        public IEnumerable<Autor> ObtenerTodos()
        {
            return _autores;
        }

        public Autor ObtenerPorId(int id)
        {
            return _autores.FirstOrDefault(a => a.ID == id);
        }

        public Autor Agregar(Autor autor)
        {
            if (_autores.Any())
            {
                autor.ID = _autores.Max(a => a.ID) + 1;
            }
            else
            {
                autor.ID = 1;
            }

            _autores.Add(autor);

            return autor;
        }

        public bool Actualizar(Autor autor)
        {
            var autorExistente = ObtenerPorId(autor.ID);

            if (autorExistente == null)
            {
                return false;
            }

            autorExistente.Nombre = autor.Nombre;
            autorExistente.Apellido = autor.Apellido;
            autorExistente.Nacionalidad = autor.Nacionalidad;
            autorExistente.FechaNacimiento = autor.FechaNacimiento;
            autorExistente.Activo = autor.Activo;

            return true;
        }

        public bool Eliminar(int id)
        {
            var autor = ObtenerPorId(id);

            if (autor == null)
            {
                return false;
            }

            return _autores.Remove(autor);
        }
    }
}