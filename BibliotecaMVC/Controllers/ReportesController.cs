using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;
using System.Globalization;
using ClosedXML.Excel;

namespace BibliotecaMVC.Controllers
{
    public class ReportesController : Controller
    {
        private readonly BibliotecaContext _context;

        public ReportesController(BibliotecaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ObtenerLibros()
        {
            var libros = await _context.Libros
                .ToListAsync();

            return Json(libros);
        }

        public async Task<IActionResult> Reporte()
        {
            var archivo = await GenerarReportePDF();

            return File(archivo, "application/pdf");
        }

        public async Task<IActionResult> DescargarReporte()
        {
            var archivo = await GenerarReportePDF();

            return File(archivo, "application/pdf", "ReporteLibros.pdf");
        }

        private async Task<byte[]> GenerarReportePDF()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var libros = await _context.Libros
                .OrderBy(l => l.ID)
                .ToListAsync();

            var documento = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(35);
                    page.PageColor(Colors.White);

                    page.Header().Column(column =>
                    {
                        column.Item().Text("BIBLIOTECA UNIVERSITARIA")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().Text("Reporte de libros")
                            .FontSize(24)
                            .Bold();

                        column.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(10);

                        column.Item().PaddingTop(10)
                            .LineHorizontal(2)
                            .LineColor(Colors.Blue.Medium);
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2)
                                    .Padding(6).Text("ID").Bold().FontColor(Colors.White);

                                header.Cell().Background(Colors.Blue.Darken2)
                                    .Padding(6).Text("Libro").Bold().FontColor(Colors.White);

                                header.Cell().Background(Colors.Blue.Darken2)
                                    .Padding(6).Text("Autor").Bold().FontColor(Colors.White);

                                header.Cell().Background(Colors.Blue.Darken2)
                                    .Padding(6).Text("Categoría").Bold().FontColor(Colors.White);
                            });

                            foreach (var libro in libros)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).Text(libro.ID.ToString());

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).Text(libro.Titulo);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).Text(libro.Autor);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).Text(libro.Categoria);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            });

            return documento.GeneratePdf();
        }
        public async Task<IActionResult> GenerarPDF()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var libros = await _context.Libros
                .OrderBy(l => l.ID)
                .ToListAsync();

            var documento = Document.Create(document =>
            {
                document.Page(page =>
                {
                   
                });
            });

            byte[] archivo = documento.GeneratePdf();

           
            return File(archivo, "application/pdf", "ReporteLibros.pdf");
        }

        public async Task<IActionResult> ExportarExcel()
        {
            var libros = await _context.Libros
                .OrderBy(l => l.ID)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Reporte de Libros");

            worksheet.Cell("A1").Value = "ID";
            worksheet.Cell("B1").Value = "Nombre del libro";
            worksheet.Cell("C1").Value = "Autor";
            worksheet.Cell("D1").Value = "Categoría";

            var encabezado = worksheet.Range("A1:D1");

            encabezado.Style.Font.Bold = true;
            encabezado.Style.Font.FontColor = XLColor.White;
            encabezado.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            encabezado.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            int fila = 2;

            foreach (var libro in libros)
            {
                worksheet.Cell(fila, 1).Value = libro.ID;
                worksheet.Cell(fila, 2).Value = libro.Titulo;
                worksheet.Cell(fila, 3).Value = libro.Autor;
                worksheet.Cell(fila, 4).Value = libro.Categoria;

                fila++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ReporteLibros.xlsx"
            );
        }
    }
}