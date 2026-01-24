using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Validators;
using FluentValidation;

namespace FacturacionVERIFACTU.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IValidator<CrearClienteDto> _crearValidator;
        private readonly IValidator<ActualizarClienteDto> _actualizarValidator;

        public ClientesController(
             ApplicationDbContext context,
             ITenantContext tenantContext,
             IValidator<CrearClienteDto> crearValidator,
             IValidator<ActualizarClienteDto> actualizarValidator)
        {
            _context = context;
            _tenantContext = tenantContext;
            _crearValidator = crearValidator;
            _actualizarValidator = actualizarValidator;
        }

        ///<summary>
        ///Obtiene lista paginada de clientes con busqueda opcional
        /// </summary>
        [HttpGet] 
        public async Task<ActionResult<PaginatedResponseDto<ClienteResponseDto>>> GetClientes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Clientes
                .Where(c => c.TenantId == tenantId.Value)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c=>
                    c.NIF.ToLower().Contains(searchLower) ||
                    c.Nombre.ToLower().Contains(searchLower));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClienteResponseDto
                { 
                    ClienteId = c.Id,
                    NIF = c.NIF,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    CodigoPostal = c.CodigoPostal,
                    Poblacion = c.Ciudad,
                    Provincia = c.Provincia,
                    Pais = c.Pais,
                    Email = c.Email,
                    Telefono = c.Telefono,
                    FechaCreaccion = c.FechaCreacion,
                    FechaModificacion = c.FechaModificacion,
                    Activo = c.Activo,
                    RegimenRecargoEquivalencia = c.RegimenRecargoEquivalencia,
                    PorcentajeRetencionDefecto = c.PorcentajeRetencionDefecto ?? 0,
                    TipoCliente = c.TipoCliente,
                    NotasFiscales = c.NotasFiscales
                })
                .ToListAsync();

            var response = new PaginatedResponseDto<ClienteResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un cliente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteResponseDto>> GetCliente(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var cliente = await _context.Clientes
                .Where(c => c.Id == id && c.TenantId == tenantId.Value)
                .Select(c => new ClienteResponseDto
                {
                    ClienteId = c.Id,
                    NIF = c.NIF,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    CodigoPostal = c.CodigoPostal,
                    Poblacion = c.Ciudad,
                    Provincia = c.Provincia,
                    Pais = c.Pais,
                    Email = c.Email,
                    Telefono = c.Telefono,
                    FechaCreaccion = c.FechaCreacion,
                    FechaModificacion = c.FechaModificacion,
                    Activo = c.Activo,
                    RegimenRecargoEquivalencia = c.RegimenRecargoEquivalencia,
                    PorcentajeRetencionDefecto = c.PorcentajeRetencionDefecto ?? 0,
                    TipoCliente = c.TipoCliente,
                    NotasFiscales = c.NotasFiscales
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(cliente);
        }

        ///<summary>
        ///Crear un nuevo cliente
        /// </summary>

        [HttpPost]
        public async Task<ActionResult<ClienteResponseDto>> CrearCliente(CrearClienteDto dto)
        {
            //Validar con FluentValidation
            var validationResult = await _crearValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var exiteNIF = await _context.Clientes
                .AnyAsync(c=> c.TenantId == tenantId.Value && c.NIF == dto.NIF);

            if (exiteNIF)
                return Conflict(new { message = "Ya existe un cliente con ese NIF" });

            var cliente = new Cliente
            {
                TenantId = tenantId.Value,
                NIF = dto.NIF,
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                CodigoPostal = dto.CodigoPostal,
                Ciudad = dto.Poblacion,
                Provincia = dto.Provincia,
                Pais = dto.Pais ?? "España",
                Email = dto.Email,
                Telefono = dto.Telefono,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow,
                Activo = dto.Activo,
                RegimenRecargoEquivalencia = dto.RegimenRecargoEquivalencia,
                PorcentajeRetencionDefecto = dto.PorcentajeRetencionDefecto,
                TipoCliente = dto.TipoCliente,
                NotasFiscales = dto.NotasFiscales
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var response = new ClienteResponseDto
            {
                ClienteId = cliente.Id,
                NIF = cliente.NIF,
                Nombre = cliente.Nombre,
                Direccion = cliente.Direccion,
                CodigoPostal = cliente.CodigoPostal,
                Poblacion = cliente.Ciudad,
                Provincia = cliente.Provincia,
                Pais = cliente.Pais,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaCreaccion = cliente.FechaCreacion,
                FechaModificacion = cliente.FechaModificacion,
                Activo = cliente.Activo,
                RegimenRecargoEquivalencia = cliente.RegimenRecargoEquivalencia,
                PorcentajeRetencionDefecto = cliente.PorcentajeRetencionDefecto ?? 0,
                TipoCliente = cliente.TipoCliente,
                NotasFiscales = cliente.NotasFiscales
                
            };

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, response);
        }

        ///<summary>
        ///Actualiza un cliente existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ClienteResponseDto>> ActualizarCliente (int id, ActualizarClienteDto dto)
        {
            //Validar con FluentValidation
            var validationResult = await _actualizarValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            cliente.Nombre = dto.Nombre;
            cliente.Direccion = dto.Direccion;
            cliente.CodigoPostal = dto.CodigoPostal;
            cliente.Ciudad = dto.Poblacion;
            cliente.Provincia = dto.Provincia;
            cliente.Pais = dto.Pais;
            cliente.Email = dto.Email;
            cliente.Telefono = dto.Telefono;
            cliente.FechaModificacion = DateTime.UtcNow;
            cliente.Activo = dto.Activo;
            cliente.RegimenRecargoEquivalencia = dto.RegimenRecargoEquivalencia;
            cliente.PorcentajeRetencionDefecto = dto.PorcentajeRetencionDefecto;
            cliente.TipoCliente = dto.TipoCliente;
            cliente.NotasFiscales = dto.NotasFiscales;

            await _context.SaveChangesAsync();

            var response = new ClienteResponseDto
            {
                ClienteId = cliente.Id,
                NIF = cliente.NIF,
                Nombre = cliente.Nombre,
                Direccion = cliente.Direccion,
                CodigoPostal = cliente.CodigoPostal,
                Poblacion = cliente.Ciudad,
                Provincia = cliente.Provincia,
                Pais = cliente.Pais,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaCreaccion = cliente.FechaCreacion,
                FechaModificacion = cliente.FechaModificacion,
                Activo = cliente.Activo,
                RegimenRecargoEquivalencia = cliente.RegimenRecargoEquivalencia,
                PorcentajeRetencionDefecto = cliente.PorcentajeRetencionDefecto ?? 0,
                TipoCliente = cliente.TipoCliente,
                NotasFiscales = cliente.NotasFiscales
            };

            return Ok(response);
                
        }

        ///<summary>
        ///Elimina un cliente
        ///</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarCliente(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no encontrado" });

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            var tieneFacturas = await _context.Facturas
                .AnyAsync(f => f.ClienteId == cliente.Id);

            if (tieneFacturas)
                return BadRequest(new { message = "No se puede eliminar el cliente porque tiene facturas asociadas" });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
                    
        }
    }
}
