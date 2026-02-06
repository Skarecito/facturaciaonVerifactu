using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Xml.Linq;

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
            string tipoDocumento = DocumentTypes.PRESUPUESTO);
    }

    public class SerieNumeracionService : ISerieNumeracionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SerieNumeracionService> _logger;


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
            string tipoDocumento = DocumentTypes.PRESUPUESTO)
        {
            // Lock para evitar condiciones de carrera
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Buscar configuración de serie
                var serieNumeracion = await _context.SeriesNumeracion
                    .FromSqlInterpolated($@"
                        SELECT * FROM series_facturacion
                        WHERE tenant_id = {tenantId}
                          AND codigo = {codigoSerie}
                          AND ejercicio = {ejercicio}
                          AND tipo_documento = {tipoDocumento}
                        FOR UPDATE")
                    .FirstOrDefaultAsync();

                if (serieNumeracion == null)
                {
                    var descripcion = $"Serir {codigoSerie} {tipoDocumento}";
                    const string formatoPorDefecto = "{SERIE}-{NUMERO}/{EJERCICIO}";

                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO series_facturacion
                            (tenant_id, codigo, descripcion, tipo_documento, proximo_numero, ejercicio, formato, activo, bloqueada)
                        VALUES
                            ({tenantId}, {codigoSerie}, {descripcion}, {tipoDocumento}, 1, {ejercicio}, {formatoPorDefecto}, true, false)
                        ON CONFLICT (tenant_id, codigo, ejercicio, tipo_documento) DO NOTHING;");
                }

                // Obtener número actual
                int numeroActual = serieNumeracion.ProximoNumero;

                // Incrementar para el próximo
                serieNumeracion.ProximoNumero++;

                // Guardar cambios
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Formatear número completo: P2024-001
                var numeroCompleto = $"{serieNumeracion.Codigo}{ejercicio}-{numeroActual:D3}";

                _logger.LogInformation(
                    "Generado número {NumeroCompleto} para tenant {TenantId}, serie {Codigo}, ejercicio {Ejercicio}",
                    numeroCompleto, tenantId, codigoSerie, ejercicio);

                return (numeroCompleto, numeroActual);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Error al obtener siguiente número para tenant {TenantId}, serie {Codigo}, ejercicio {Ejercicio}",
                    tenantId, codigoSerie, ejercicio);
                throw;
            }
        }
    }
}
