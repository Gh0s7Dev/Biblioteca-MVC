using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly string _connectionString;

        public CategoriasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Categorias
        public IActionResult Index()
        {
            var categorias = new List<Categoria>();

            using (var conexion = new SqlConnection(_connectionString))
            {
                var sql = "SELECT ID, Nombre, Descripcion FROM Categorias";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    conexion.Open();

                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            categorias.Add(new Categoria
                            {
                                ID = lector.GetInt32(0),
                                Nombre = lector.GetString(1),
                                Descripcion = lector.IsDBNull(2)
                                    ? null
                                    : lector.GetString(2)
                            });
                        }
                    }
                }
            }

            return View(categorias);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria categoria)
        {
            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                ModelState.AddModelError(
                    "Nombre",
                    "El nombre de la categoría es obligatorio."
                );

                return View(categoria);
            }

            using (var conexion = new SqlConnection(_connectionString))
            {
                var sql = @"
                    INSERT INTO Categorias (Nombre, Descripcion)
                    VALUES (@Nombre, @Descripcion)";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@Nombre",
                        categoria.Nombre
                    );

                    comando.Parameters.AddWithValue(
                        "@Descripcion",
                        (object)categoria.Descripcion ?? DBNull.Value
                    );

                    conexion.Open();
                    comando.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Categoria guardada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Edit/5
        public IActionResult Edit(int id)
        {
            Categoria categoria = null;

            using (var conexion = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT ID, Nombre, Descripcion
                    FROM Categorias
                    WHERE ID = @ID";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", id);

                    conexion.Open();

                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            categoria = new Categoria
                            {
                                ID = lector.GetInt32(0),
                                Nombre = lector.GetString(1),
                                Descripcion = lector.IsDBNull(2)
                                    ? null
                                    : lector.GetString(2)
                            };
                        }
                    }
                }
            }

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categorias/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Categoria categoria)
        {
            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                ModelState.AddModelError(
                    "Nombre",
                    "El nombre de la categoría es obligatorio."
                );

                return View(categoria);
            }

            using (var conexion = new SqlConnection(_connectionString))
            {
                var sql = @"
                    UPDATE Categorias
                    SET Nombre = @Nombre,
                        Descripcion = @Descripcion
                    WHERE ID = @ID";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@Nombre",
                        categoria.Nombre
                    );

                    comando.Parameters.AddWithValue(
                        "@Descripcion",
                        (object)categoria.Descripcion ?? DBNull.Value
                    );

                    comando.Parameters.AddWithValue(
                        "@ID",
                        categoria.ID
                    );

                    conexion.Open();

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas == 0)
                    {
                        return NotFound();
                    }
                }
            }

            TempData["SuccessMessage"] =
                "Categoria actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Delete/5
        public IActionResult Delete(int id)
        {
            Categoria categoria = null;

            using (var conexion = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT ID, Nombre, Descripcion
                    FROM Categorias
                    WHERE ID = @ID";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", id);

                    conexion.Open();

                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            categoria = new Categoria
                            {
                                ID = lector.GetInt32(0),
                                Nombre = lector.GetString(1),
                                Descripcion = lector.IsDBNull(2)
                                    ? null
                                    : lector.GetString(2)
                            };
                        }
                    }
                }
            }

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categorias/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                var sql = @"
                    DELETE FROM Categorias
                    WHERE ID = @ID";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", id);

                    conexion.Open();

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas == 0)
                    {
                        return NotFound();
                    }
                }
            }

            TempData["SuccessMessage"] =
                "Categoria eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}