using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
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

            // 2. Crear Cierre de Ejercicio Actual (abierto)
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

        var series = new List<SerieNumeracion>
        {
            // Serie de Facturas
            new SerieNumeracion
            {
                TenantId = tenantId,
                Codigo = "A",
                TipoDocumento = "Factura",
                Ejercicio = anoActual,
                ProximoNumero = 1,
                Formato = "{SERIE}-{NUMERO}/{EJERCICIO}",
                Descripcion = "Serie A de facturas",
                Activo = true
            },

            // Serie de Albaranes
            new SerieNumeracion
            {
                TenantId = tenantId,
                Codigo = "A",
                TipoDocumento = "Albaran",
                Ejercicio = anoActual,
                ProximoNumero = 1,
                Formato = "{SERIE}-{NUMERO}/{EJERCICIO}",
                Descripcion = "Serie A de albaranes",
                Activo = true
            },

            // Serie de Presupuestos
            new SerieNumeracion
            {
                TenantId = tenantId,
                Codigo = "A",
                TipoDocumento = "Presupuesto",
                Ejercicio = anoActual,
                ProximoNumero = 1,
                Formato = "{SERIE}-{NUMERO}/{EJERCICIO}",
                Descripcion = "Serie principal de presupuestos",
                Activo = true
            },

            // Serie de Facturas Rectificativas
            new SerieNumeracion
            {
                TenantId = tenantId,
                Codigo = "R",
                TipoDocumento = "Factura",
                Ejercicio = anoActual,
                ProximoNumero = 1,
                Formato = "{SERIE}-{NUMERO}/{EJERCICIO}",
                Descripcion = "Serie de facturas rectificativas",
                Activo = true
            }
        };

        await _context.SeriesNumeraciones.AddRangeAsync(series);

        _logger.LogInformation("Creadas {Count} series de numeración para tenant {TenantId}",
            series.Count, tenantId);
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
}