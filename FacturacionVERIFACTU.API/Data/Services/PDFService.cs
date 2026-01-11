using API.Data.Entities;
using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Migrations;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Net.Http.Headers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel;

namespace FacturacionVERIFACTU.API.Data.Services
{
    public interface IPDFService
    {
        Task<byte[]> GenerarPDFPresupuesto(int presupuestoId, int tenantID);
        Task<byte[]> GenerarPDFAlbaran(int albaranId, int tenantID);
        Task<byte[]> GenerarPDFFactura(int facturaId, int tenantId);
    }
    public class PDFService : IPDFService
    {
        private readonly ApplicationDbContext _context;

        public PDFService(ApplicationDbContext context)
        {
            _context = context;
            //Licencia community de QuesPdf
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerarPDFPresupuesto(int presupuestoId,int tenantId)
        {
            //!. Obtener presupuesto completo
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Cliente)
                .Include(p => p.Tenant)
                .Include(p => p.Serie)
                .Include(p => p.Lineas.OrderBy(l => l.Orden))
                .ThenInclude(l => l.Producto)
                .FirstOrDefaultAsync(p => p.Id == presupuestoId && p.TenantId == tenantId);

            if (presupuesto == null)
                throw new InvalidOperationException($"Prespuesto {presupuestoId} no encontrado");

            // 2. Generar PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => GenerarEncabezadoPresupuesto(c, presupuesto));
                    page.Content().Element(c => GenerarContenidoPresupuesto(c, presupuesto));
                    page.Footer().Element(c => GenerarPiePresupuesto(c, presupuesto));
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerarPDFAlbaran(int albaranId, int tenantId)
        {
            var albaran = await _context.Albaranes
                .Include(a => a.Cliente)
                .Include(a => a.Tenant)
                .Include(a => a.Serie)
                .Include(a => a.Lineas.OrderBy(l => l.Orden))
                    .ThenInclude(l => l.Producto)
                .FirstOrDefaultAsync(a => a.Id == albaranId && a.TenantId == tenantId);

            if (albaran == null)
                throw new InvalidOperationException($"Albaran {albaranId} no encontrado");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => GenerarEncabezadoAlbaran(c, albaran));
                    page.Content().Element(c => GenerarContenidoAlbaran(c, albaran));
                    page.Footer().Element(c => GenerarPieAlbaran(c, albaran));
                });
            });

            return document.GeneratePdf();
        }


        public async Task<byte[]> GenerarPDFFactura(int facturaId, int tenantId)
        {
            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Tenant)
                .Include(f => f.Serie)
                .Include(f => f.Lineas.OrderBy(l => l.Orden))
                    .ThenInclude(l => l.Producto)
                .FirstOrDefaultAsync(f => f.Id==facturaId && f.TenantId==tenantId);

            if (factura == null)
                throw new InvalidOperationException($"Factura {facturaId} no encontrada");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => GenerarEncabezadoFactura(c, factura));
                    page.Content().Element(c => GenerarContenidoFactura(c, factura));
                    page.Footer().Element(c => GenerarPieFactura(c, factura));
                });
            });

            return document.GeneratePdf();
        }

        // ========================================
        // PRESUPUESTO - Componentes
        // ========================================
        private void GenerarEncabezadoPresupuesto(QuestPDF.Infrastructure.IContainer container, Presupuesto presupuesto)
        {
            container.Column(col =>
            {
                //Fila superior: Logo +  Empresa VS Presupuesto
                {
                    col.Item().Row(row =>
                    {
                        //Logo (izquierda si existe
                        if(presupuesto.Tenant.Logo != null && presupuesto.Tenant.Logo.Length > 0)
                        {
                            row.ConstantItem(80).AlignLeft().AlignMiddle()
                                .Image(presupuesto.Tenant.Logo)
                                .FitArea();
                        }

                        //Datos empresa(centro-izquierda)
                        row.RelativeItem().PaddingLeft(10).Column(c =>
                        {
                            c.Item().Text(presupuesto.Tenant.Nombre)
                                .FontSize(16).Bold().FontColor("#1e3a8a");
                            c.Item().Text($"CIF/NIF: {presupuesto.Tenant.NIF}").FontSize(9);
                            c.Item().Text(presupuesto.Tenant.Direccion ?? "").FontSize(9);
                            c.Item().Text($"{presupuesto.Tenant.CodigoPostal} {presupuesto.Tenant.Ciudad}");
                            c.Item().Text($"{presupuesto.Tenant.Provincia}").FontSize(9);
                            if (!string.IsNullOrEmpty(presupuesto.Tenant.Telefono))
                                c.Item().Text($"{presupuesto.Tenant.Telefono}").FontSize(9);
                            if(!string.IsNullOrEmpty(presupuesto.Tenant.Email))
                                c.Item().Text($"{presupuesto.Tenant.Email}").FontSize (9);
                        });

                        //Numero prespupuesto derecha
                        row.ConstantItem(180).Column(c =>
                        {
                            c.Item().AlignRight().Text("PRESUPUESTO")
                                .FontSize(20).Bold().FontColor("1e3a8a");
                            c.Item().AlignRight().Text($"{presupuesto.Serie.Codigo} - {presupuesto.Numero}")
                                .FontSize(14).Bold();
                            c.Item().AlignRight().Text($"Fecha: {presupuesto.Fecha:dd/MM/yyyy}")
                                .FontSize(9).Italic();
                        });
                    });

                    col.Item().Padding(15).LineHorizontal(1).LineColor("#cbd5e1");

                    //Datos del cliente
                    col.Item().PaddingTop(15).Column(c =>
                    {
                        c.Item().Text("CLIENTE").FontSize(11).Bold().FontColor("#475569");
                        c.Item().PaddingTop(5).Text(presupuesto.Cliente.Nombre)
                            .FontSize(12).Bold();
                        c.Item().Text($"CIF/NIF: {presupuesto.Cliente.NIF}").FontSize(9);
                        c.Item().Text($"{presupuesto.Cliente.CodigoPostal} {presupuesto.Cliente.Ciudad}").FontSize(9);
                        c.Item().Text($"{presupuesto.Cliente.Provincia}").FontSize(9);
                        if (!string.IsNullOrEmpty(presupuesto.Cliente.Telefono))
                            c.Item().Text($"{presupuesto.Cliente.Telefono}").FontSize(9);
                        if (!string.IsNullOrEmpty(presupuesto.Cliente.Email))
                            c.Item().Text($"{presupuesto.Cliente.Email}").FontSize(9);
                    });

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#cbd5e1");
                }
            });
        }

        private void GenerarContenidoPresupuesto(QuestPDF.Infrastructure.IContainer container, Presupuesto presupuesto)
        {
            container.PaddingTop(20).Column(col =>
            {
                //Tabla de lineas
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); //Nº
                        columns.RelativeColumn(4); //Descripcion
                        columns.ConstantColumn(60); //Cantidad
                        columns.ConstantColumn(70); //Precio
                        columns.ConstantColumn(50); //IVA
                        columns.ConstantColumn(50); //Req Equiv
                        columns.ConstantColumn(80); // Total
                    });

                    //Cabecera
                    table.Header(header =>
                    {
                        header.Cell().Background("#1e3a8a").Padding(5)
                            .Text("N").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background(("#1e2a8a")).Padding(5)
                            .Text("Descripcion").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#1e3a8a").Padding(5)
                            .Text("Cantidad").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#1e3a8a").Padding(5).AlignRight()
                            .Text("Precio").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#1e3a8a").Padding(5).AlignRight()
                            .Text("IVA %").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#1e3a8a").Padding(5).AlignRight()
                            .Text("Rec %").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#1e3a8a").Padding(5).AlignRight()
                            .Text("Total").FontColor("#ffffff").FontSize(9).Bold();
                    });

                    //Lineas
                    int i = 1;
                    foreach (var linea in presupuesto.Lineas)
                    {
                        var bgColor = i % 2 == 0 ? "#f8fafc" : "#ffffff";

                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5)
                            .Text(i.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5)
                            .Text(linea.Descripcion.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight()
                            .Text(linea.Cantidad.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight()
                            .Text(linea.PrecioUnitario.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight()
                            .Text($"{linea.IVA:N2}%").FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight()
                            .Text($"{linea.RecargoEquivalencia:N2}%").FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight()
                            .Text(linea.TotalLinea.ToString("C")).FontSize(9);

                        i++;
                    }
                });

                //Totales
                col.Item().PaddingTop(15).AlignRight().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(120).Text("Base Imponible:").FontSize(10).AlignRight();
                        r.ConstantItem(100).Text(presupuesto.BaseImponible.ToString("C"))
                            .FontSize(10).Bold().AlignRight();
                    });

                    c.Item().PaddingTop(3).Row(r =>
                    {
                        r.ConstantItem(120).Text("Total IVA:").FontSize(10).AlignRight();
                        r.ConstantItem(100).Text(presupuesto.TotalIva.ToString("C"))
                            .FontSize(10).Bold().AlignRight();
                    });

                    if (presupuesto.TotalRecargo.HasValue && presupuesto.TotalRecargo > 0)
                    {
                        c.Item().PaddingTop(3).Row(r =>
                        {
                            r.ConstantItem(120).Text("Total Rec Eq.").FontSize(10).AlignRight();
                            r.ConstantItem(100).Text(presupuesto.TotalRecargo.Value.ToString("C"))
                                .FontSize(10).Bold().AlignRight();
                        });
                    }

                    if (presupuesto.CuotaRetencion.HasValue && presupuesto.CuotaRetencion > 0)
                    {
                        c.Item().PaddingTop(3).Row(r =>
                        {
                            r.ConstantItem(120).Text($"Retencion ({presupuesto.PorcentajeRetencion}%):").FontSize(9).AlignRight();
                            r.ConstantItem(100).Text($"-{presupuesto.CuotaRetencion.Value:C}");
                        });
                    }

                    c.Item().PaddingTop(5).LineHorizontal(1).LineColor("#1e3a8a");

                    c.Item().PaddingTop(5).Row(r =>
                    {
                        r.ConstantItem(120).Text("TOTAL:").FontSize(12).Bold().AlignRight();
                        r.ConstantItem(100).Text((presupuesto.TotalConRetencion ?? presupuesto.Total).ToString("C"))
                            .FontSize(12).Bold().FontColor("#1e3a8a").AlignRight();
                    });
                });
            });
        }

        private void GenerarPiePresupuesto(QuestPDF.Infrastructure.IContainer container, Presupuesto presupuesto)
        {
            container.Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(presupuesto.Observaciones))
                {
                    col.Item().PaddingTop(20).Column(c =>
                    {
                        c.Item().Text("Observaciones:").FontSize(9).Bold().FontColor("#475569");
                        c.Item().PaddingTop(3).Text(presupuesto.Observaciones).FontSize(8).Italic();
                    });
                }

                col.Item().AlignCenter().PaddingTop(10).Text($"Estado: {presupuesto.Estado}")
                    .FontSize(8).FontColor("#64748b");

                col.Item().AlignCenter().Text("Gracias por su confianza")
                    .FontSize(8).FontColor("#94a3b8");
            });
        }


        // ========================================
        // ALBARÁN - Componentes
        // ========================================
        private void GenerarEncabezadoAlbaran(QuestPDF.Infrastructure.IContainer container, Albaran albaran)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // LOGO (izquierda) - si existe
                    if (albaran.Tenant.Logo != null && albaran.Tenant.Logo.Length > 0)
                    {
                        row.ConstantItem(80).AlignLeft().AlignTop()
                            .Image(albaran.Tenant.Logo)
                            .FitArea();
                    }

                    // Datos empresa
                    row.RelativeItem().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text(albaran.Tenant.Nombre)
                            .FontSize(16).Bold().FontColor("#059669");
                        c.Item().Text($"CIF: {albaran.Tenant.NIF}").FontSize(9);
                        c.Item().Text(albaran.Tenant.Direccion ?? "").FontSize(9);
                        c.Item().Text($"{albaran.Tenant.CodigoPostal} {albaran.Tenant.Ciudad}").FontSize(9);
                        if (!string.IsNullOrEmpty(albaran.Tenant.Telefono))
                            c.Item().Text($"Tel: {albaran.Tenant.Telefono}").FontSize(9);
                        if (!string.IsNullOrEmpty(albaran.Tenant.Email))
                            c.Item().Text(albaran.Tenant.Email).FontSize(9);
                    });

                    row.ConstantItem(180).Column(c =>
                    {
                        c.Item().AlignRight().Text("ALBARÁN")
                            .FontSize(20).Bold().FontColor("#059669");
                        c.Item().AlignRight().Text($"{albaran.Serie.Codigo}-{albaran.Numero}")
                            .FontSize(14).Bold();
                        c.Item().AlignRight().Text($"Fecha: {albaran.FechaEmision:dd/MM/yyyy}")
                            .FontSize(9);
                    });
                });

                col.Item().PaddingTop(15).LineHorizontal(1).LineColor("#cbd5e1");

                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("CLIENTE").FontSize(11).Bold().FontColor("#475569");
                    c.Item().PaddingTop(5).Text(albaran.Cliente.Nombre).FontSize(12).Bold();
                    c.Item().Text($"CIF/NIF: {albaran.Cliente.NIF}").FontSize(9);
                    c.Item().Text(albaran.Cliente.Direccion ?? "").FontSize(9);

                    if (!string.IsNullOrWhiteSpace(albaran.DireccionEntrega))
                    {
                        c.Item().PaddingTop(5).Text("Dirección de entrega:")
                            .FontSize(9).Bold().FontColor("#059669");
                        c.Item().Text(albaran.DireccionEntrega).FontSize(9);
                    }
                });

                col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#cbd5e1");
            });
        }


        private void GenerarContenidoAlbaran(QuestPDF.Infrastructure.IContainer container, Albaran albaran)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Item().Table(table =>
                {
                    // Tabla SIN precios - solo descripción y cantidad
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // Nº
                        columns.RelativeColumn();     // Descripción - más espacio sin precios
                        columns.ConstantColumn(80);   // Cantidad
                        columns.ConstantColumn(100);  // Unidad (opcional)
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#059669").Padding(5)
                            .Text("Nº").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#059669").Padding(5)
                            .Text("Descripción").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#059669").Padding(5).AlignRight()
                            .Text("Cantidad").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#059669").Padding(5).AlignCenter()
                            .Text("Unidad").FontColor("#ffffff").FontSize(9).Bold();
                    });

                    int i = 1;
                    foreach (var linea in albaran.Lineas)
                    {
                        var bgColor = i % 2 == 0 ? "#f0fdf4" : "#ffffff";

                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#d1fae5").Padding(5)
                            .Text(i.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#d1fae5").Padding(5)
                            .Text(linea.Descripcion).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#d1fae5").Padding(5).AlignRight()
                            .Text(linea.Cantidad.ToString("N2")).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#d1fae5").Padding(5).AlignCenter()
                            .Text("Ud.").FontSize(9); // Unidad genérica

                        i++;
                    }
                });

                // SIN TOTALES - solo información adicional si hay observaciones
                if (!string.IsNullOrWhiteSpace(albaran.Observaciones))
                {
                    col.Item().PaddingTop(20).Column(c =>
                    {
                        c.Item().Text("Observaciones:").FontSize(9).Bold().FontColor("#475569");
                        c.Item().PaddingTop(3).Text(albaran.Observaciones).FontSize(9).Italic();
                    });
                }

                // Firmas (opcional - común en albaranes)
                col.Item().PaddingTop(40).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().LineHorizontal(1).LineColor("#cbd5e1");
                        c.Item().PaddingTop(5).AlignCenter()
                            .Text("Firma y sello empresa").FontSize(8).FontColor("#64748b");
                    });

                    row.ConstantItem(40); // Espacio

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().LineHorizontal(1).LineColor("#cbd5e1");
                        c.Item().PaddingTop(5).AlignCenter()
                            .Text("Firma y sello cliente (recibí)").FontSize(8).FontColor("#64748b");
                    });
                });
            });
        }

        private void GenerarPieAlbaran(QuestPDF.Infrastructure.IContainer container, Albaran albaran)
        {
            container.Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(albaran.Observaciones))
                {
                    col.Item().PaddingTop(20).Column(c =>
                    {
                        c.Item().Text("Observaciones:").FontSize(9).Bold().FontColor("#475569");
                        c.Item().PaddingTop(3).Text(albaran.Observaciones).FontSize(8).Italic();
                    });
                }

                col.Item().AlignCenter().PaddingTop(10).Text($"Estado: {albaran.Estado}")
                    .FontSize(8).FontColor("#64748b");
            });
        
        }


        // ========================================
        // FACTURA - Componentes (CON QR VERIFACTU)
        // ========================================
        private void GenerarEncabezadoFactura(QuestPDF.Infrastructure.IContainer container, Factura factura)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    //Logo (izquierda si existe
                    if (factura.Tenant.Logo != null && factura.Tenant.Logo.Length > 0)
                    {
                        row.ConstantItem(80).AlignLeft().AlignTop()
                            .Image(factura.Tenant.Logo)
                            .FitArea();
                    }

                    //Datos empresa(centro-izquierda)
                    row.RelativeItem().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text(factura.Tenant.Nombre)
                            .FontSize(16).Bold().FontColor("#dc2626");
                        c.Item().Text($"CIF/NIF: {factura.Tenant.NIF}").FontSize(9);
                        c.Item().Text(factura.Tenant.Direccion ?? "").FontSize(9);
                        c.Item().Text($"{factura.Tenant.CodigoPostal} {factura.Tenant.Ciudad}");
                        c.Item().Text($"{factura.Tenant.Provincia}").FontSize(9);
                        if (!string.IsNullOrEmpty(factura.Tenant.Telefono))
                            c.Item().Text($"{factura.Tenant.Telefono}").FontSize(9);
                        if (!string.IsNullOrEmpty(factura.Tenant.Email))
                            c.Item().Text($"{factura.Tenant.Email}").FontSize(9);
                    });

                    row.ConstantItem(180).Column(c =>
                    {
                        c.Item().AlignRight().Text("FACTURA")
                                .FontSize(20).Bold().FontColor("dc2626");
                        c.Item().AlignRight().Text($"{factura.Serie.Codigo} - {factura.Numero}")
                            .FontSize(14).Bold();
                        c.Item().AlignRight().Text($"Fecha: {factura.FechaEmision:dd/MM/yyyy}")
                            .FontSize(9).Italic();

                        //Indicador VERIFACTU
                        if (factura.EnviadaVERIFACTU)
                        {
                            c.Item().PaddingTop(5).AlignRight()
                                .Background("dcfce7").Padding(3)
                                .Text("✓ VERI*FACTU")
                                .FontSize(8).Bold().FontColor("#166534");
                        }
                    });
                });

                col.Item().PaddingTop(15).LineHorizontal(1).LineColor("#cbd5e1");

                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("CLIENTE").FontSize(11).Bold().FontColor("#475569");
                    c.Item().PaddingTop(5).Text(factura.Cliente.Nombre)
                        .FontSize(12).Bold();
                    c.Item().Text($"CIF/NIF: {factura.Cliente.NIF}").FontSize(9);
                    c.Item().Text($"{factura.Cliente.CodigoPostal} {factura.Cliente.Ciudad}").FontSize(9);
                    c.Item().Text($"{factura.Cliente.Provincia}").FontSize(9);
                    if (!string.IsNullOrEmpty(factura.Cliente.Telefono))
                        c.Item().Text($"{factura.Cliente.Telefono}").FontSize(9);
                    if (!string.IsNullOrEmpty(factura.Cliente.Email))
                        c.Item().Text($"{factura.Cliente.Email}").FontSize(9);
                });

                col.Item().Padding(10).LineHorizontal(1).LineColor("#cbd5e1");
            });
        }

        private void GenerarContenidoFactura(QuestPDF.Infrastructure.IContainer container, Factura factura)
        {
            container.PaddingTop(20).Column(col =>
            {
                //Tabla de lineas
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); //Nº
                        columns.RelativeColumn(4); //Descripcion
                        columns.ConstantColumn(60); //Cantidad
                        columns.ConstantColumn(70); //Precio
                        columns.ConstantColumn(50); //IVA
                        columns.ConstantColumn(50); //Req Equiv
                        columns.ConstantColumn(80); // Total
                    });

                    //Cabecera
                    table.Header(header =>
                    {
                        header.Cell().Background("#dc2626").Padding(5)
                            .Text("N").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background(("#dc2626")).Padding(5)
                            .Text("Descripcion").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#dc2626").Padding(5)
                            .Text("Cantidad").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#dc2626").Padding(5).AlignRight()
                            .Text("Precio").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#dc2626").Padding(5).AlignRight()
                            .Text("IVA %").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#dc2626").Padding(5).AlignRight()
                            .Text("Rec %").FontColor("#ffffff").FontSize(9).Bold();
                        header.Cell().Background("#dc2626").Padding(5).AlignRight()
                            .Text("Total").FontColor("#ffffff").FontSize(9).Bold();
                    });

                    //Lineas
                    int i = 1;
                    foreach (var linea in factura.Lineas)
                    {
                        var bgColor = i % 2 == 0 ? "#fef2f2" : "#ffffff";

                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5)
                            .Text(i.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5)
                            .Text(linea.Descripcion.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5).AlignRight()
                            .Text(linea.Cantidad.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5).AlignRight()
                            .Text(linea.PrecioUnitario.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5).AlignRight()
                            .Text($"{linea.IVA:N2}%").FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5).AlignRight()
                            .Text($"{linea.RecargoEquivalencia:N2}%").FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#fecaca").Padding(5).AlignRight()
                            .Text(linea.TotalLinea.ToString("C")).FontSize(9);

                        i++;
                    }
                });

   
            

            // Totales + Qr Code
            col.Item().PaddingTop(15).Row(row =>
                {

                    //Columna izquierda : QR VERIFACTU (si existe)
                    if (factura.EnviadaVERIFACTU && factura.QRVerifactu != null && factura.QRVerifactu.Length > 0)
                    {
                        row.ConstantItem(100).Column(c =>
                        {
                            c.Item().AlignCenter().Text("QR VERI*FACTU")
                                .FontSize(8).Bold().FontColor("#166534");
                            c.Item().PaddingTop(5).AlignCenter()
                                .Image(factura.QRVerifactu)
                                .FitWidth();

                            if (factura.FechaEnvioVERIFACTU.HasValue)
                            {
                                c.Item().PaddingTop(3).AlignCenter()
                                    .Text($"Enviado: {factura.FechaEnvioVERIFACTU.Value:dd/MM/yyyy HH:mm}");
                            }
                        });

                        row.RelativeItem(); //Espacio
                    }
                    else
                    {
                        row.RelativeItem();
                    }

                    //Columna derecha: Totales
                    row.ConstantItem(220).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.ConstantItem(120).Text("Base Imponible:").FontSize(10).AlignRight();
                            r.ConstantItem(100).Text(factura.BaseImponible.ToString("C"))
                            .FontSize(10).Bold().AlignRight();
                        });

                        c.Item().PaddingTop(3).Row(r =>
                        {
                            r.ConstantItem(120).Text("Total IVA:").FontSize(10).AlignRight();
                            r.ConstantItem(100).Text(factura.TotalIVA.ToString("C")).AlignRight();
                        });

                        if (factura.TotalRecargo.HasValue && factura.TotalRecargo > 0)
                        {
                            c.Item().PaddingTop(3).Row(r =>
                            {
                                r.ConstantItem(120).Text("Total Rec. Eq.:").FontSize(10).AlignRight();
                                r.ConstantItem(100).Text(factura.TotalRecargo.Value.ToString("C"))
                                    .FontSize(10).Bold().AlignRight();
                            });
                        }

                        if (factura.CuotaRetencion.HasValue && factura.CuotaRetencion > 0)
                        {
                            c.Item().PaddingTop(3).Row(r =>
                            {
                                r.ConstantItem(120).Text($"Retencion ({factura.PorcentajeRetencion}%):").FontSize(9).AlignRight();
                                r.ConstantItem(100).Text($"-{factura.CuotaRetencion.Value:C}")
                                    .FontSize(9).FontColor("#dc2626").AlignRight();
                            });
                        }

                        c.Item().PaddingTop(5).LineHorizontal(1).LineColor("#dc2626");

                        c.Item().PaddingTop(5).Row(r =>
                        {
                            r.ConstantItem(120).Text("TOTAL:").FontSize(12).Bold().AlignRight();
                            r.ConstantItem(100).Text(factura.Total.ToString("C"))
                                .FontSize(12).Bold().FontColor("#dc2626").AlignRight();
                        });
                    });

                });
            });
        }

        private void GenerarPieFactura(QuestPDF.Infrastructure.IContainer container, Factura factura)
        {
            container.Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(factura.Observaciones))
                {
                    col.Item().PaddingTop(20).Column(c =>
                    {
                        c.Item().Text("Observaciones:").FontSize(9).Bold().FontColor("#475569");
                        c.Item().PaddingTop(3).Text(factura.Observaciones).FontSize(8).Italic();
                    });
                }

                // ============================================
                // DATOS REGISTRO MERCANTIL (obligatorio en facturas)
                // ============================================
                if (!string.IsNullOrEmpty(factura.Tenant.RegistroMercantil))
                {
                    col.Item().PaddingTop(15).Column(c =>
                    {
                        c.Item().LineHorizontal(0.5f).LineColor("#cbd5e1");

                        c.Item().PaddingTop(8).AlignCenter().Text("DATOS REGISTRALES")
                            .FontSize(8).Bold().FontColor("#475569");

                        // Construir texto de registro mercantil
                        var datosRegistro = factura.Tenant.RegistroMercantil;

                        var detalles = new List<string>();
                        if (!string.IsNullOrEmpty(factura.Tenant.Tomo))
                            detalles.Add($"Tomo {factura.Tenant.Tomo}");
                        if (!string.IsNullOrEmpty(factura.Tenant.Libro))
                            detalles.Add($"Libro {factura.Tenant.Libro}");
                        if (!string.IsNullOrEmpty(factura.Tenant.Folio))
                            detalles.Add($"Folio {factura.Tenant.Folio}");
                        if (!string.IsNullOrEmpty(factura.Tenant.Seccion))
                            detalles.Add($"Sección {factura.Tenant.Seccion}");
                        if (!string.IsNullOrEmpty(factura.Tenant.Hoja))
                            detalles.Add($"Hoja {factura.Tenant.Hoja}");
                        if (!string.IsNullOrEmpty(factura.Tenant.Inscripcion))
                            detalles.Add($"Inscripción {factura.Tenant.Inscripcion}");

                        if (detalles.Any())
                            datosRegistro += " - " + string.Join(", ", detalles);

                        c.Item().PaddingTop(3).AlignCenter()
                            .Text(datosRegistro)
                            .FontSize(7).FontColor("#64748b");
                    });

                    // Informacion VERIFACTU
                    if (factura.EnviadaVERIFACTU)
                    {
                        col.Item().PaddingTop(10).Column(c =>
                        {
                            c.Item().AlignCenter().Text("✓ Factura certificada VERI * FACTU")
                                .FontSize(9).Bold().FontColor("#166537");

                            if (!string.IsNullOrWhiteSpace(factura.Huella))
                            {
                                c.Item().AlignCenter().Text($"Huella: {factura.Huella}")
                                    .FontSize(7).FontColor("#64748b");
                            }

                            if (!string.IsNullOrWhiteSpace(factura.UrlVERIFACTU))
                            {
                                c.Item().AlignCenter().Text($"Verificar en: {factura.UrlVERIFACTU}")
                                    .FontSize(7).FontColor("#2563eb");
                            }
                        });
                    }
                }

                col.Item().AlignCenter().PaddingTop(10).Text($"Estado: {factura.Estado}")
                    .FontSize(8).FontColor("#64748b");
            });
        }
    }
}
