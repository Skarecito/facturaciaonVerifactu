using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.JSInterop.Infrastructure;

namespace FacturacionVERIFACTU.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger _logger;

        public TenantsController(
            ApplicationDbContext context,
            ITenantContext tenantContext,
            ILogger<TenantsController> logger)
        {
            _context = context;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        ///<summary>
        /// Obtiene los datos del Tenant actual
        /// </summary>
        [HttpGet("mi-empresa")]
        [ProducesResponseType(typeof(TenantResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TenantResponseDto>> ObtenerMiEmpresa()
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null)
                    return Unauthorized(new { mensaje = "Tenant no identificado" });

                var tenant = await _context.Tenants.FindAsync(tenantId.Value);
                if (tenant == null)
                    return NotFound(new { mensaje = "Empresa no encontrada" });

                return Ok(new TenantResponseDto
                {
                    Id = tenant.Id,
                    RazonSocial = tenant.Nombre,
                    NIF = tenant.NIF,
                    Direccion = tenant.Direccion,
                    CodigoPostal = tenant.CodigoPostal,
                    Poblacion = tenant.Ciudad,
                    Provincia = tenant.Provincia,
                    Telefono = tenant.Telefono,
                    Email = tenant.Email,
                    TieneLogo = tenant.Logo != null && tenant.Logo.Length > 0,
                    RegistroMercantil = tenant.RegistroMercantil,
                    Tomo = tenant.Tomo,
                    Libro = tenant.Libro,
                    Folio = tenant.Folio,
                    Seccion = tenant.Seccion,
                    Hoja = tenant.Hoja,
                    Inscripcion = tenant.Inscripcion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de la empresa");
                return StatusCode(500, new { mensaje = "Error al obtener datos" });
            }
        }

        ///<summary>
        /// Actualiza los datos del tenant actual
        /// </summary>
        [HttpPut("mi-empresa")]
        [ProducesResponseType(typeof(TenantResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ActualizarMiEmpresa([FromBody] TenantUpdateDto dto)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                    return Unauthorized(new { mensaje = "Tenant no identificado" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenant = await _context.Tenants.FindAsync(tenantId.Value);
                if (tenant == null)
                    return NotFound(new { mensaje = "Empresa no encontrada" });

                //Actualizar datos
                tenant.Nombre = dto.RazonSocial;
                tenant.NIF = dto.NIF;
                tenant.Direccion = dto.Direccion;
                tenant.CodigoPostal = dto.CodigoPostal;
                tenant.Ciudad = dto.Poblacion;
                tenant.Provincia = dto.Provincia;
                tenant.Telefono = dto.Telefono;
                tenant.Email = dto.Email;
                tenant.RegistroMercantil = dto.RegistroMercantil;
                tenant.Tomo = dto.Tomo;
                tenant.Libro = dto.Libro;
                tenant.Folio = dto.Folio;
                tenant.Seccion = dto.Seccion;
                tenant.Hoja = dto.Hoja;
                tenant.Inscripcion = dto.Inscripcion;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Datos actualizados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actuliazar datos de la empresa");
                return StatusCode(500, new { mensaje = "Error al actualizar datos" });
            }
        }

        ///<summary>
        /// Sube el logo de la empresa(png/jpg) max 2MB
        /// </summary>
        [HttpPost("mi-empresa/logo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubirLogo(IFormFile archivo)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                    return Unauthorized(new { mensaje = "Tenant no encontrado" });

                //Validaciones
                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new { mensaje = "No se ha enviado ningun archivo" });

                if (archivo.Length > 2 * 1024 * 1024) //2MB
                    return BadRequest(new { mensaje = "El archivo es demasiado grande (max 2MB)" });

                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                if (!extensionesPermitidas.Contains(extension))
                    return BadRequest(new { mensaje = "Solo se permiten imagenes JPG o PNG" });

                //Leer archivo
                using var memoryStream = new MemoryStream();
                await archivo.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                //Actualizar Tenant
                var tenant = await _context.Tenants.FindAsync(tenantId.Value);
                if (tenant == null)
                    return NotFound(new { mensaje = "Empresa no encontrada" });

                tenant.Logo = bytes;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Logo subido correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir el logo");
                return StatusCode(500, new { mensaje = "Error al subir el logo" });
            }
        }

        ///<sumary>
        /// Descarga el logo de la empresa
        /// </sumary>
        [HttpGet("mi-empresa/logo")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarLogo()
        {
            try
            {
                var tenantid = _tenantContext.GetTenantId();
                if (tenantid == null || tenantid == 0)
                    return Unauthorized(new { mensaje = "Tenant no encontrado" });

                var tenant = await _context.Tenants.FindAsync(tenantid.Value);
                if (tenant == null || tenant.Logo == null || tenant.Logo.Length == 0)
                    return NotFound(new { mensaje = "Logo no encontrado" });

                return File(tenant.Logo, "image/png", "logo.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar el logo");
                return StatusCode(500, new { mensaje = "Error al descargar el logo" });
            }
        }


        ///<summary>
        /// Elimina el logo de la empresa
        /// </summary>
        [HttpDelete("mi_empresa/logo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarLogo()
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                    return Unauthorized(new { mensaje = "Tenant no identificado" });

                var tenant = await _context.Tenants.FindAsync(tenantId.Value);
                if (tenant == null)
                    return NotFound(new { mensaje = "Empresa no encontrada" });

                tenant.Logo = null;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Logo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el logo");
                return StatusCode(500, new { mensaje = "Error al eliminar el logo" });
            }
        }
    }
}
