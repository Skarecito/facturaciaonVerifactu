using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FacturacionVERIFACTU.API.Data.Services;



public class TenantInitializationService : ITenantInitializationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TenantInitializationService> _logger;

    public TenantInitializationService(
        ApplicationDbContext context,
        ILogger<TenantInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task IncicializarTenantAsync(int tenantId)
    {
        _logger.LogInformation("Iniciando inicialización del tenant {TenantId}", tenantId);

        try
        {
            // 1. Crear Series de Numeración
            await CrearSeriesNumeracionAsync(tenantId);

            // 2. Crear Tipos de Impuesto
            await CrearTiposImpuestoAsync(tenantId);

            // 3. Crear Cierre de Ejercicio Actual (abierto)
            await CrearCierreEjercicioInicialAsync(tenantId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant {TenantId} inicializado correctamente", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inicializando tenant {TenantId}", tenantId);
            throw;
        }
    }

    private async Task CrearSeriesNumeracionAsync(int tenantId)
    {
        var anoActual = DateTime.Now.Year;

        // 1. Verificar cuáles ya existen ANTES de crear nada
        var seriesExistentes = await _context.SeriesNumeracion
            .Where(s => s.TenantId == tenantId && s.Ejercicio == anoActual)
            .Select(s => new { s.Codigo, TipoDoc = s.TipoDocumento.ToUpperInvariant() })
            .ToListAsync();

        var existentesSet = new HashSet<string>(
            seriesExistentes.Select(s => $"{s.Codigo}|{s.TipoDoc}"));

        // 2. Definir series por defecto
        var series = new List<SerieNumeracion>
    {
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "F",
            TipoDocumento = DocumentTypes.FACTURA,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie F de facturas",
            Activo        = true
        },
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "A",
            TipoDocumento = DocumentTypes.ALBARAN,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie A de albaranes",
            Activo        = true
        },
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "P",
            TipoDocumento = DocumentTypes.PRESUPUESTO,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie principal de presupuestos",
            Activo        = true
        },
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "R",
            TipoDocumento = DocumentTypes.FACTURA,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie de facturas rectificativas",
            Activo        = true
        }
    };

        // 3. Filtrar solo las que NO existen ya
        var seriesNuevas = series
            .Where(s => !existentesSet.Contains(
                $"{s.Codigo}|{s.TipoDocumento.ToUpperInvariant()}"))
            .ToList();

        if (seriesNuevas.Count == 0)
        {
            _logger.LogInformation(
                "Series de numeración ya existentes para tenant {TenantId} y ejercicio {Ano}",
                tenantId, anoActual);
            return;
        }

        await _context.SeriesNumeracion.AddRangeAsync(seriesNuevas);

        _logger.LogInformation("Creadas {Count} series de numeración para tenant {TenantId}",
            seriesNuevas.Count, tenantId);
    }


    private async Task CrearCierreEjercicioInicialAsync(int tenantId)
    {
        var anoActual = DateTime.Now.Year;

        // Verificar si ya existe
        var cierreExistente = await _context.CierreEjercicios
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ejercicio == anoActual);

        if (cierreExistente != null)
        {
            _logger.LogInformation("Cierre de ejercicio {Ano} ya existe para tenant {TenantId}",
                anoActual, tenantId);
            return;
        }

        // Crear cierre inicial (abierto, sin datos)
        var cierre = new CierreEjercicio
        {
            TenantId = tenantId,
            Ejercicio = anoActual,
            FechaCierre = DateTime.UtcNow, // Aún no está cerrado
            UsuarioId = tenantId,   // Se asignará cuando se cierre
            HashFinal = string.Empty,
            TotalFacturas = 0,
            TotalBaseImponible = 0,
            TotalIVA = 0,
            TotalRecargo = 0,
            TotalRetencion = 0,
            TotalImporte = 0,
            RutaLibroFacturas = null,
            RutaResumenIVA = null,
            EnviadoVERIFACTU = false,
            FechaEnvio = null,
            EstaAbierto = true, // ← IMPORTANTE: Ejercicio abierto
            MotivoReapertura = null,
            FechaReapertura = null,
            UsuarioReaperturaId = null,
            CreadoEn = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow
        };

        await _context.CierreEjercicios.AddAsync(cierre);

        _logger.LogInformation("Creado cierre de ejercicio {Ano} (abierto) para tenant {TenantId}",
            anoActual, tenantId);
    }

    private async Task CrearTiposImpuestoAsync(int tenantId)
    {
        var tiposExistentes = await _context.TiposImpuesto
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.Nombre)
            .ToListAsync();

        var tiposPorDefecto = new List<TipoImpuesto>
        {
            new TipoImpuesto
            {
                TenantId = tenantId,
                Nombre = "General",
                PorcentajeIva = 21m,
                PorcentajeRecargo = 5.2m,
                Activo = true,
                Orden = 1
            },
            new TipoImpuesto
            {
                TenantId = tenantId,
                Nombre = "Reducido",
                PorcentajeIva = 10m,
                PorcentajeRecargo = 1.4m,
                Activo = true,
                Orden = 2
            },
            new TipoImpuesto
            {
                TenantId = tenantId,
                Nombre = "Superreducido",
                PorcentajeIva = 4m,
                PorcentajeRecargo = 0.5m,
                Activo = true,
                Orden = 3
            },
            new TipoImpuesto
            {
                TenantId = tenantId,
                Nombre = "Exento",
                PorcentajeIva = 0m,
                PorcentajeRecargo = 0m,
                Activo = true,
                Orden = 4
            }
        };

        var nuevosTipos = tiposPorDefecto
            .Where(t => !tiposExistentes.Contains(t.Nombre))
            .ToList();

        if (!nuevosTipos.Any())
        {
            _logger.LogInformation("Tipos de impuesto ya inicializados para tenant {TenantId}", tenantId);
            return;
        }

        await _context.TiposImpuesto.AddRangeAsync(nuevosTipos);

        _logger.LogInformation("Creados {Count} tipos de impuesto para tenant {TenantId}", nuevosTipos.Count, tenantId);
    }
}
