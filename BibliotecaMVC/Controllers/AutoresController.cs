using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using BibliotecaMVC.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<AutoresController> _logger;

        public AutoresController(BibliotecaContext context, ILogger<AutoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Consultando el listado de autores");

            try
            {
                var autores = _context.Autores.ToList();
                return View(autores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al consultar el listado de autores.");
                throw;   
            }
        }

        public async Task<IActionResult> Reporte()
        {
            var autores = await _context.Autores
                   .AsNoTracking()
                   .OrderBy(a => a.Nombre)
                   .ToListAsync();

            return File(GenerarReporte(autores), "application/pdf");
        }

        public async Task<IActionResult> DescargarReporte()
        {
            var autores = await _context.Autores
                   .AsNoTracking()
                   .OrderBy(a => a.Nombre)
                   .ToListAsync();
            
            return File(GenerarReporte(autores), "application/pdf", "reporte-autores.pdf");
        }

        private static byte[] GenerarReporte(IReadOnlyCollection<Autor> autores)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(document => document.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(42);
                page.PageColor(Colors.Grey.Lighten5);

                page.Header().Column(column =>
                {
                    column.Item().Text("BIBLIOTECA UNIVERSITARIA")
                    .FontSize(11)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Reporte de autores")
                    .FontSize(26)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);
                    column.Item().Text($"Listado actualizado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(14).LineHorizontal(2).LineColor(Colors.Blue.Medium);
                });
                page.Content().PaddingVertical(22).Column(column =>
                {
                    column.Item().Background(Colors.Blue.Darken2).Padding(14).Row(row =>
                    {
                        row.RelativeItem().Text("Autores Registrados")
                        .FontSize(11)
                        .Bold()
                        .FontColor(Colors.White);
                        row.ConstantItem(45).AlignRight().Text(autores.Count.ToString())
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.White);
                    });

                    column.Item().PaddingTop(18).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(42);
                            columns.RelativeColumn(2.4f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.3f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("ID");
                            header.Cell().Element(HeaderCellStyle).Text("NOMBRE");
                            header.Cell().Element(HeaderCellStyle).Text("NACIONALIDAD");
                            header.Cell().Element(HeaderCellStyle).Text("NACIMIENTO");
                        });

                        foreach (var autor in autores)
                        {
                            table.Cell().Element(BodyCellStyle).Text(autor.ID.ToString());
                            table.Cell().Element(BodyCellStyle).Text(autor.Nombre);
                            table.Cell().Element(BodyCellStyle).Text(autor.Nacionalidad ?? "No especificada");
                            table.Cell().Element(BodyCellStyle).Text(autor.FechaNacimiento.ToString("dd/MM/yyyy"));
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Biblioteca Universitaria - Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            })).GeneratePdf();

            static IContainer HeaderCellStyle(IContainer container) => container
                .Background(Colors.Blue.Darken2)
                .PaddingVertical(9)
                .PaddingHorizontal(8)
                .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.White));

            static IContainer BodyCellStyle(IContainer container) => container
                .Background(Colors.White)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(9)
                .PaddingHorizontal(8)
                .DefaultTextStyle(x => x.FontSize(9).Bold().FontColor(Colors.Grey.Darken3));
        }

        public IActionResult Details(int id)
        {
            _logger.LogInformation("Consultando detalles de un autor con identificador {AutorId}", id);
            var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

            try
            {
                if (autor == null)
                {
                    _logger.LogWarning("No se encontró un autor con identificador {AutorId}", id);
                    return NotFound();
                }

                return View(autor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al consultar los detalles del autor con identificador {AutorId}.", id);
                throw;
            }
        }

        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operación de creación de un autor");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Create(Autor autor)
        {
            _logger.LogInformation("Iniciando la operación de creación de un autor");
            if (!ModelState.IsValid)
            {
                _logger.LogError("El autor enviado no cumple con los requerimientos mínimos");
                return View(autor);
            }

            try
            {
                _context.Autores.Add(autor);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al crear el autor.");
                throw;
            }
        }
        
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public IActionResult Edit(int id)
        {
            _logger.LogInformation("Iniciando operación de edición de un autor con identificador {AutorId}", id);
            try
            {
                var autor = _context.Autores.FirstOrDefault(a => a.ID == id);

                if (autor == null)
                {
                    return NotFound();
                }

                return View(autor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al consultar los detalles del autor para su edición con identificador {AutorId}.", id);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Edit(int id, Autor autor)
        {
            _logger.LogInformation("Iniciando la edición del autor con identificador {AutorId}", id);
            if (id != autor.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            try
            {
                var exist = await _context.Autores.AnyAsync(a => a.ID == id);
                if(!exist)
                {
                    _logger.LogWarning("No se encontró un autor con identificador {AutorId} para editar", id);
                    return NotFound();
                }

                _context.Update(autor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al editar el autor con identificador {AutorId}.", id);
                throw;
            }
        }

        // POST: Autores/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, Bibliotecario")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Iniciando la eliminación del autor con identificador {AutorId}", id);
            try
            {
                var autor = await _context.Autores.FindAsync(id);

                if (autor == null)
                {
                    _logger.LogWarning("No se encontró un autor con identificador {AutorId} para eliminar", id);
                    return NotFound();
                }

                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al eliminar el autor con identificador {AutorId}.", id);
                throw;
            }
        }
    }
}