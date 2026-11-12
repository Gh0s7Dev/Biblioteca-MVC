
using BibliotecaMVC.Controllers;
using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BibliotecaMVC.Tests
{
    public class AutoresControllerTests
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

        private static AutoresController CreateController(BibliotecaContext context)
        {
            var logger = new Mock<ILogger<AutoresController>>();

            return new AutoresController(context, logger.Object);
        }

        private static Autor CrearAutor(
            string nombre = "Juan",
            string apellido = "Pérez",
            string nacionalidad = "Salvadoreña")
        {
            return new Autor
            {
                Nombre = nombre,
                Apellido = apellido,
                Nacionalidad = nacionalidad
            };
        }

        [Fact]
        public async Task Index_DevuelveLaListaDeAutores()
        {
            using var context = CreateContext();
            context.Autores.Add(CrearAutor());
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Index();

            var vista = Assert.IsType<ViewResult>(resultado);
            var autores = Assert.IsAssignableFrom<IEnumerable<Autor>>(vista.Model);

            Assert.Single(autores);
        }

        [Fact]
        public async Task Details_ConAutorExistente_DevuelveElAutor()
        {
            using var context = CreateContext();

            var autor = CrearAutor();
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Details(autor.ID);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<Autor>(vista.Model);

            Assert.Equal(autor.ID, modelo.ID);
        }

        [Fact]
        public void Details_ConAutorInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var resultado = controller.Details(999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public void Create_Get_DevuelveLaVista()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var resultado = controller.Create();

            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public void Create_Post_ConAutorValido_GuardaYRedirige()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var autor = CrearAutor();

            var resultado = controller.Create(autor);

            var redireccion = Assert.IsType<RedirectToActionResult>(resultado);

            Assert.Equal("Index", redireccion.ActionName);
            Assert.Single(context.Autores);
        }

        [Fact]
        public void Create_Post_ConModeloInvalido_NoGuardaYDevuelveVista()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var autor = CrearAutor();
            controller.ModelState.AddModelError("Nombre", "Nombre inválido");

            var resultado = controller.Create(autor);

            var vista = Assert.IsType<ViewResult>(resultado);

            Assert.Equal(autor, vista.Model);
            Assert.Empty(context.Autores);
        }

        [Fact]
        public async Task Edit_Get_ConAutorExistente_DevuelveLaVista()
        {
            using var context = CreateContext();

            var autor = CrearAutor();
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = controller.Edit(autor.ID);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<Autor>(vista.Model);

            Assert.Equal(autor.ID, modelo.ID);
        }

        [Fact]
        public void Edit_Get_ConAutorInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var resultado = controller.Edit(999);

            Assert.IsType<NotFoundResult>(resultado);
        }


        [Fact]
        public async Task Edit_Post_ConAutorValido_ActualizaYRedirige()
        {
            var databaseName = Guid.NewGuid().ToString();

            int autorId;

            // Crear y guardar el autor inicial.
            using (var context = CreateContext(databaseName))
            {
                var autor = CrearAutor("Juan", "Pérez", "Salvadoreña");

                context.Autores.Add(autor);
                await context.SaveChangesAsync();

                autorId = autor.ID;
            }

            // Utilizar un contexto nuevo para evitar que EF Core
            // tenga otra instancia del autor en seguimiento.
            using (var context = CreateContext(databaseName))
            {
                var autorEditado = new Autor
                {
                    ID = autorId,
                    Nombre = "Pedro",
                    Apellido = "Gómez",
                    Nacionalidad = "Mexicana"
                };

                var controller = CreateController(context);

                var resultado = await controller.Edit(autorId, autorEditado);

                var redireccion = Assert.IsType<RedirectToActionResult>(resultado);

                Assert.Equal("Index", redireccion.ActionName);
            }

            // Verificar que los cambios quedaron guardados.
            using (var context = CreateContext(databaseName))
            {
                var autorGuardado = await context.Autores.FirstAsync();

                Assert.Equal("Pedro", autorGuardado.Nombre);
                Assert.Equal("Gómez", autorGuardado.Apellido);
                Assert.Equal("Mexicana", autorGuardado.Nacionalidad);
            }
        }


        [Fact]
        public async Task Edit_Post_ConIdentificadoresDiferentes_DevuelveNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var autor = CrearAutor();
            autor.ID = 10;

            var resultado = await controller.Edit(5, autor);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Edit_Post_ConAutorInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var autor = CrearAutor();
            autor.ID = 999;

            var resultado = await controller.Edit(999, autor);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Delete_ConAutorExistente_EliminaYRedirige()
        {
            using var context = CreateContext();

            var autor = CrearAutor();
            context.Autores.Add(autor);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var resultado = await controller.Delete(autor.ID);

            var redireccion = Assert.IsType<RedirectToActionResult>(resultado);

            Assert.Equal("Index", redireccion.ActionName);
            Assert.Empty(context.Autores);
        }

        [Fact]
        public async Task Delete_ConAutorInexistente_DevuelveNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var resultado = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(resultado);
        }
    }
}
