
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(
            IConfiguration configuration,
            ILogger<CategoriasController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        // GET: Categorias
        public IActionResult Index()
        {
            _logger.LogInformation("Iniciando consulta de todas las categorías.");

            var categorias = new List<Categoria>();

            try
            {
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

                _logger.LogInformation(
                    "Consulta de categorías finalizada. Registros encontrados: {Cantidad}",
                    categorias.Count);

                return View(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar las categorías.");
                throw;
            }
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Acceso al formulario de creación de categorías.");

            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria categoria)
        {
            _logger.LogInformation("Iniciando operación de creación de categoría.");

            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                _logger.LogWarning(
                    "No se pudo crear la categoría porque el nombre está vacío.");

                ModelState.AddModelError(
                    "Nombre",
                    "El nombre de la categoría es obligatorio."
                );

                return View(categoria);
            }

            try
            {
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

                _logger.LogInformation(
                    "Categoría creada correctamente: {NombreCategoria}",
                    categoria.Nombre);

                TempData["SuccessMessage"] = "Categoria guardada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoría.");
                throw;
            }
        }

        // GET: Categorias/Edit/5
        public IActionResult Edit(int id)
        {
            _logger.LogInformation(
                "Iniciando consulta para editar la categoría ID: {CategoriaId}",
                id);

            Categoria categoria = null;

            try
            {
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
                    _logger.LogWarning(
                        "No se encontró la categoría ID: {CategoriaId} para editar.",
                        id);

                    return NotFound();
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar la categoría ID: {CategoriaId} para editar.",
                    id);

                throw;
            }
        }

        // POST: Categorias/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Categoria categoria)
        {
            _logger.LogInformation(
                "Iniciando operación de edición de categoría ID: {CategoriaId}",
                categoria?.ID);

            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                _logger.LogWarning(
                    "No se pudo editar la categoría porque el nombre está vacío.");

                ModelState.AddModelError(
                    "Nombre",
                    "El nombre de la categoría es obligatorio."
                );

                return View(categoria);
            }

            try
            {
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
                            _logger.LogWarning(
                                "No se encontró la categoría ID: {CategoriaId} para actualizar.",
                                categoria.ID);

                            return NotFound();
                        }
                    }
                }

                _logger.LogInformation(
                    "Categoría actualizada correctamente. ID: {CategoriaId}",
                    categoria.ID);

                TempData["SuccessMessage"] =
                    "Categoria actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al actualizar la categoría ID: {CategoriaId}",
                    categoria.ID);

                throw;
            }
        }

        // GET: Categorias/Delete/5
        public IActionResult Delete(int id)
        {
            _logger.LogInformation(
                "Iniciando consulta para eliminar la categoría ID: {CategoriaId}",
                id);

            Categoria categoria = null;

            try
            {
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
                    _logger.LogWarning(
                        "No se encontró la categoría ID: {CategoriaId} para eliminar.",
                        id);

                    return NotFound();
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consultar la categoría ID: {CategoriaId} para eliminar.",
                    id);

                throw;
            }
        }

        // POST: Categorias/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _logger.LogInformation(
                "Iniciando eliminación de la categoría ID: {CategoriaId}",
                id);

            try
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
                            _logger.LogWarning(
                                "No se encontró la categoría ID: {CategoriaId} para eliminar.",
                                id);

                            return NotFound();
                        }
                    }
                }

                _logger.LogInformation(
                    "Categoría eliminada correctamente. ID: {CategoriaId}",
                    id);

                TempData["SuccessMessage"] =
                    "Categoria eliminada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar la categoría ID: {CategoriaId}",
                    id);

                throw;
            }
        }
    }
}
