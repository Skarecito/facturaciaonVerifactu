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
             ITenantContext tentantContext,
             IValidator<CrearClienteDto> crearValidator,
             IValidator<ActualizarClienteDto> actualizarValidator)
        {
            _context = context;
            _tenantContext = tentantContext;
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
                    FechaCreaccion = c.FechaAlta,
                    FechaModificacion = c.FechaModificacion
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
                    FechaCreacion = c.FechaCreacion,
                    FechaModificacion = c.FechaModificacion
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(cliente);
        }

    }
}
