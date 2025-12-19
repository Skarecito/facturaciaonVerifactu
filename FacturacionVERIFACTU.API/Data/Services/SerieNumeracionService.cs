using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.Data;
using Microsoft.EntityFrameworkCore;

namespace FacturacionVERIFACTU.API.Data.Services
{
    /// <summary>
    /// Servicio para gestion de numeracion de documentos
    /// </summary>
    public interface ISerieNumeracionService
    {
        Task<(string NumeroCompleto, int Numero)> ObtenerSiguienteNumeroAsync(
            int tenantId,
            string serie,
            int ejercicio,
            string tipoDocumento = "PRESUPUESTO");
    }

    public class SerieNumeracionService : ISerieNumeracionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SerieNumeracionService> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public SerieNumeracionService(ApplicationDbContext context, ILogger<SerieNumeracionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Obtiene el siguiente numero de una serie de forma atomica
        /// </summary>
        public async Task<(string NumeroCompleto, int Numero)> ObtenerSiguienteNumeroAsync(
            int tenantId,
            string codigoSerie,  // Código como "P", "A", etc.
            int ejercicio,
            string tipoDocumento = "PRESUPUESTO")
        {
            // Lock para evitar condiciones de carrera
            await _semaphore.WaitAsync();

            try
            {
                // Buscar configuración de serie
                var serieNumeracion = await _context.SeriesNumeracion
                    .FirstOrDefaultAsync(cs =>
                        cs.TenantId == tenantId &&
                        cs.Codigo == codigoSerie &&
                        cs.Ejercicio == ejercicio &&
                        cs.TipoDocumento == tipoDocumento);

                if (serieNumeracion == null)
                {
                    // Crear nueva configuración de serie
                    serieNumeracion = new SerieNumeracion
                    {
                        TenantId = tenantId,
                        Codigo = codigoSerie,
                        Descripcion = $"Serie {codigoSerie} {tipoDocumento}",
                        TipoDocumento = tipoDocumento,
                        ProximoNumero = 1,
                        Ejercicio = ejercicio,
                        Activo = true
                    };
                    _context.SeriesNumeracion.Add(serieNumeracion);
                }

                // Obtener número actual
                int numeroActual = serieNumeracion.ProximoNumero;

                // Incrementar para el próximo
                serieNumeracion.ProximoNumero++;

                // Guardar cambios
                await _context.SaveChangesAsync();

                // Formatear número completo: P2024-001
                var numeroCompleto = $"{serieNumeracion.Codigo}{ejercicio}-{numeroActual:D3}";

                _logger.LogInformation(
                    "Generado número {NumeroCompleto} para tenant {TenantId}, serie {Codigo}, ejercicio {Ejercicio}",
                    numeroCompleto, tenantId, codigoSerie, ejercicio);

                return (numeroCompleto, numeroActual);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al obtener siguiente número para tenant {TenantId}, serie {Codigo}, ejercicio {Ejercicio}",
                    tenantId, codigoSerie, ejercicio);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
    }
