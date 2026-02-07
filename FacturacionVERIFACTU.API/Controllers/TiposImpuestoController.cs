using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionVERIFACTU.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TiposImpuestoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TiposImpuestoController(ApplicationDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<TipoImpuestoResponseDto>>> GetTiposImpuesto()
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var tipos = await _context.TiposImpuesto
                .Where(t => t.TenantId == tenantId.Value)
                .OrderBy(t => t.Orden ?? 0)
                .ThenBy(t => t.Nombre)
                .Select(t => new TipoImpuestoResponseDto
                {
                    Id = t.Id,
                    TenantId = t.TenantId,
                    Nombre = t.Nombre,
                    PorcentajeIva = t.PorcentajeIva,
                    PorcentajeRecargo = t.PorcentajeRecargo,
                    Activo = t.Activo,
                    Orden = t.Orden,
                    FechaInicio = t.FechaInicio,
                    FechaFin = t.FechaFin
                })
                .ToListAsync();

            return Ok(tipos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoImpuestoResponseDto>> GetTipoImpuesto(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var tipo = await _context.TiposImpuesto
                .Where(t => t.Id == id && t.TenantId == tenantId.Value)
                .Select(t => new TipoImpuestoResponseDto
                {
                    Id = t.Id,
                    TenantId = t.TenantId,
                    Nombre = t.Nombre,
                    PorcentajeIva = t.PorcentajeIva,
                    PorcentajeRecargo = t.PorcentajeRecargo,
                    Activo = t.Activo,
                    Orden = t.Orden,
                    FechaInicio = t.FechaInicio,
                    FechaFin = t.FechaFin
                })
                .FirstOrDefaultAsync();

            if (tipo == null)
                return NotFound(new { message = "Tipo de impuesto no encontrado" });

            return Ok(tipo);
        }

        [HttpPost]
        public async Task<ActionResult<TipoImpuestoResponseDto>> CrearTipoImpuesto(TipoImpuestoCreateDto dto)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var existeNombre = await _context.TiposImpuesto
                .AnyAsync(t => t.TenantId == tenantId.Value && t.Nombre == dto.Nombre);

            if (existeNombre)
                return Conflict(new { message = "Ya existe un tipo de impuesto con ese nombre" });

            var tipoImpuesto = new TipoImpuesto
            {
                TenantId = tenantId.Value,
                Nombre = dto.Nombre,
                PorcentajeIva = dto.PorcentajeIva,
                PorcentajeRecargo = dto.PorcentajeRecargo,
                Activo = dto.Activo,
                Orden = dto.Orden,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin
            };

            _context.TiposImpuesto.Add(tipoImpuesto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoImpuesto), new { id = tipoImpuesto.Id }, new TipoImpuestoResponseDto
            {
                Id = tipoImpuesto.Id,
                TenantId = tipoImpuesto.TenantId,
                Nombre = tipoImpuesto.Nombre,
                PorcentajeIva = tipoImpuesto.PorcentajeIva,
                PorcentajeRecargo = tipoImpuesto.PorcentajeRecargo,
                Activo = tipoImpuesto.Activo,
                Orden = tipoImpuesto.Orden,
                FechaInicio = tipoImpuesto.FechaInicio,
                FechaFin = tipoImpuesto.FechaFin
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TipoImpuestoResponseDto>> ActualizarTipoImpuesto(int id, TipoImpuestoUpdateDto dto)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var tipo = await _context.TiposImpuesto
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId.Value);

            if (tipo == null)
                return NotFound(new { message = "Tipo de impuesto no encontrado" });

            var nombreDuplicado = await _context.TiposImpuesto
                .AnyAsync(t => t.TenantId == tenantId.Value && t.Nombre == dto.Nombre && t.Id != id);

            if (nombreDuplicado)
                return Conflict(new { message = "Ya existe un tipo de impuesto con ese nombre" });

            var enUso = await _context.Productos
                .AnyAsync(p => p.TenantId == tenantId.Value && p.TipoImpuestoId == id)
                || await _context.LineasPresupuesto.AnyAsync(l => l.TipoImpuestoId == id && l.Presupuesto.TenantId == tenantId.Value)
                || await _context.LineasFacturas.AnyAsync(l => l.TipoImpuestoId == id && l.Factura.TenantId == tenantId.Value)
                || await _context.LineasAlbaranes.AnyAsync(l => l.TipoImpuestoId == id && l.Albaran.TenantId == tenantId.Value);

            if (enUso && (tipo.PorcentajeIva != dto.PorcentajeIva || tipo.PorcentajeRecargo != dto.PorcentajeRecargo))
            {
                return BadRequest(new
                {
                    message = "El tipo está en uso. Para cambiar porcentajes, cree un nuevo tipo."
                });
            }

            tipo.Nombre = dto.Nombre;
            tipo.PorcentajeIva = dto.PorcentajeIva;
            tipo.PorcentajeRecargo = dto.PorcentajeRecargo;
            tipo.Activo = dto.Activo;
            tipo.Orden = dto.Orden;
            tipo.FechaInicio = dto.FechaInicio;
            tipo.FechaFin = dto.FechaFin;

            await _context.SaveChangesAsync();

            return Ok(new TipoImpuestoResponseDto
            {
                Id = tipo.Id,
                TenantId = tipo.TenantId,
                Nombre = tipo.Nombre,
                PorcentajeIva = tipo.PorcentajeIva,
                PorcentajeRecargo = tipo.PorcentajeRecargo,
                Activo = tipo.Activo,
                Orden = tipo.Orden,
                FechaInicio = tipo.FechaInicio,
                FechaFin = tipo.FechaFin
            });
        }

        [HttpPost("{id}/desactivar")]
        public async Task<ActionResult> Desactivar(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var tipo = await _context.TiposImpuesto
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId.Value);

            if (tipo == null)
                return NotFound(new { message = "Tipo de impuesto no encontrado" });

            tipo.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}