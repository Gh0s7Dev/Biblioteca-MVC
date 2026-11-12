
using BibliotecaMVC.Controllers;
using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BibliotecaMVC.Tests
{
    public class LibrosControllerTests
    {
        private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

        private static BibliotecaContext CreateContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<BibliotecaContext>()
                .UseInMemoryDatabase(
                    databaseName ?? Guid.NewGuid().ToString(),
                    DatabaseRoot)
                .Options;

            return new BibliotecaContext(options);
        }

        private static LibrosController CreateController(BibliotecaContext context)
        {
            var logger = new Mock<ILogger<LibrosController>>();

            var controller = new LibrosController(context, logger.Object);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        private static Libro CrearLibro(
            string titulo = "Cien años de soledad",
            string autor = "Gabriel García Márquez",
            string categoria = "Novela",
            decimal precio = 25.50m,
            bool disponible = true)
        {
            return new Libro
            {
                Titulo = titulo,
                Autor = autor,
                Categoria = categoria,
                Precio = precio,
                Disponible = disponible
            };
        }

        // =========================
        // PRUEBAS DE INDEX
        // =========================

        [Fact]
        public async Task Index_ConLibros_DevuelveLaLista()
        {
            using var context = CreateContext();

            context.Libros.Add(CrearLibro());
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Index();

            var vista = Assert.IsType<ViewResult>(resultado);
            var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(vista.Model);

            Assert.Single(libros);
        }

        [Fact]
        public void Index_SinLibros_DevuelveListaVacia()
        {
            using var context = CreateContext();

            var controller = CreateController(context);

            var resultado = controller.Index();

            var vista = Assert.IsType<ViewResult>(resultado);
            var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(vista.Model);

            Assert.Empty(libros);
        }

        // =========================
        // PRUEBAS DE DETAILS
        // =========================

        [Fact]
        public async Task Details_ConLibroExistente_DevuelveElLibro()
        {
            using var context = CreateContext();

            var libro = CrearLibro();
            context.Libros.Add(libro);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Details(libro.ID);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<Libro>(vista.Model);

            Assert.Equal(libro.ID, modelo.ID);
            Assert.Equal("Cien años de soledad", modelo.Titulo);
        }

        [Fact]
        public void Details_ConLibroInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(context);

            var resultado = controller.Details(999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // =========================
        // PRUEBAS DE CREATE
        // =========================

        [Fact]
        public void Create_Get_DevuelveLaVista()
        {
            using var context = CreateContext();

            var controller = CreateController(context);

            var resultado = controller.Create();

            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public void Create_Post_ConLibroValido_GuardaYRedirige()
        {
            using var context = CreateContext();

            var controller = CreateController(context);
            var libro = CrearLibro();

            var resultado = controller.Create(libro);

            var redireccion = Assert.IsType<RedirectToActionResult>(resultado);

            Assert.Equal("Index", redireccion.ActionName);
            Assert.Single(context.Libros);
        }

        [Fact]
        public void Create_Post_ConModeloInvalido_NoGuardaYDevuelveVista()
        {
            using var context = CreateContext();

            var controller = CreateController(context);
            var libro = CrearLibro();

            controller.ModelState.AddModelError(
                "Titulo",
                "El título no es válido.");

            var resultado = controller.Create(libro);

            var vista = Assert.IsType<ViewResult>(resultado);

            Assert.Equal(libro, vista.Model);
            Assert.Empty(context.Libros);
        }

        // =========================
        // PRUEBAS DE EDIT GET
        // =========================

        [Fact]
        public async Task Edit_Get_ConLibroExistente_DevuelveLaVista()
        {
            using var context = CreateContext();

            var libro = CrearLibro();
            context.Libros.Add(libro);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Edit(libro.ID);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<Libro>(vista.Model);

            Assert.Equal(libro.ID, modelo.ID);
        }

        [Fact]
        public void Edit_Get_ConLibroInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(context);

            var resultado = controller.Edit(999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // =========================
        // PRUEBAS DE EDIT POST
        // =========================

        [Fact]
        public async Task Edit_Post_ConLibroValido_ActualizaYRedirige()
        {
            var databaseName = Guid.NewGuid().ToString();
            int libroId;

            // Guardar el libro original.
            using (var context = CreateContext(databaseName))
            {
                var libro = CrearLibro();

                context.Libros.Add(libro);
                await context.SaveChangesAsync();

                libroId = libro.ID;
            }

            // Actualizar utilizando un contexto independiente.
            using (var context = CreateContext(databaseName))
            {
                var libroEditado = new Libro
                {
                    ID = libroId,
                    Titulo = "El principito",
                    Autor = "Antoine de Saint-Exupéry",
                    Categoria = "Literatura",
                    Precio = 18.75m,
                    Disponible = false
                };

                var controller = CreateController(context);

                var resultado = controller.Edit(libroId, libroEditado);

                var redireccion = Assert.IsType<RedirectToActionResult>(resultado);

                Assert.Equal("Index", redireccion.ActionName);
            }

            // Verificar los cambios guardados.
            using (var context = CreateContext(databaseName))
            {
                var libroGuardado = await context.Libros.FirstAsync();

                Assert.Equal("El principito", libroGuardado.Titulo);
                Assert.Equal("Antoine de Saint-Exupéry", libroGuardado.Autor);
                Assert.Equal("Literatura", libroGuardado.Categoria);
                Assert.Equal(18.75m, libroGuardado.Precio);
                Assert.False(libroGuardado.Disponible);
            }
        }

        [Fact]
        public void Edit_Post_ConIdentificadoresDiferentes_DevuelveNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(context);
            var libro = CrearLibro();

            libro.ID = 10;

            var resultado = controller.Edit(5, libro);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public void Edit_Post_ConModeloInvalido_DevuelveVista()
        {
            using var context = CreateContext();

            var controller = CreateController(context);
            var libro = CrearLibro();

            libro.ID = 1;

            controller.ModelState.AddModelError(
                "Titulo",
                "El título no es válido.");

            var resultado = controller.Edit(1, libro);

            var vista = Assert.IsType<ViewResult>(resultado);

            Assert.Equal(libro, vista.Model);
            Assert.Empty(context.Libros);
        }

        // =========================
        // PRUEBAS DE DELETE GET
        // =========================

        [Fact]
        public async Task Delete_Get_ConLibroExistente_DevuelveLaVista()
        {
            using var context = CreateContext();

            var libro = CrearLibro();
            context.Libros.Add(libro);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Delete(libro.ID);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<Libro>(vista.Model);

            Assert.Equal(libro.ID, modelo.ID);
        }

        [Fact]
        public void Delete_Get_ConLibroInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(context);

            var resultado = controller.Delete(999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // =========================
        // PRUEBAS DE DELETE CONFIRMED
        // =========================

        [Fact]
        public async Task DeleteConfirmed_ConLibroExistente_EliminaYRedirige()
        {
            using var context = CreateContext();

            var libro = CrearLibro();
            context.Libros.Add(libro);
            await context.SaveChangesAsync();

            var libroId = libro.ID;
            var controller = CreateController(context);

            var resultado = controller.DeleteConfirmed(libroId);

            var redireccion = Assert.IsType<RedirectToActionResult>(resultado);

            Assert.Equal("Index", redireccion.ActionName);
            Assert.Empty(context.Libros);

            Assert.Equal(
                "Libro eliminado correctamente.",
                controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public void DeleteConfirmed_ConLibroInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();

            var controller = CreateController(context);

            var resultado = controller.DeleteConfirmed(999);

            Assert.IsType<NotFoundResult>(resultado);
            Assert.Empty(context.Libros);
        }
    }
}
