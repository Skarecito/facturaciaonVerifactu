using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.Data;




namespace FacturacionVERIFACTU.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IValidator<CrearProductoDto> _crerValidator;
        private readonly IValidator<ActualizarProductoDto> _actualizarValidator;

        public ProductosController(
            ApplicationDbContext context,
            ITenantContext tenantContext,
            IValidator<CrearProductoDto> crearValidator,
            IValidator<ActualizarProductoDto> actualizarValidator)
        {
            _context = context;
            _tenantContext = tenantContext;
            _crerValidator = crearValidator;
            _actualizarValidator = actualizarValidator;
        }

        /// <summary>
        /// Obtiene lista paginada de productos con búsqueda opcional
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDto<ProductoResponseDto>>> GetProductos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            // Validar parámetros
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            // Query base
            var query = _context.Productos
                .Where(p => p.TenantId == tenantId.Value)
                .AsQueryable();

            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Codigo.ToLower().Contains(searchLower) ||
                    p.Descripcion.ToLower().Contains(searchLower));
            }

            // Total items
            var totalItems = await query.CountAsync();

            // Paginación
            var items = await query
                .OrderBy(p => p.Codigo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductoResponseDto
                {
                    ProductoId = p.Id,
                    Codigo = p.Codigo,
                    Descripcion = p.Descripcion,
                    PrecioUnitario = p.Precio,
                    IVA = p.IVA,
                    Unidad = p.Unidad,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaModificacion = p.FechaModificacion
                })
                .ToListAsync();

            var response = new PaginatedResponseDto<ProductoResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };

            return Ok(response);
        }

        ///<summary>
        ///Obtiene un producto por id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoResponseDto>> GetProducto(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var producto = await _context.Productos
                .Where(p => p.Id == id && p.TenantId == tenantId.Value)
                .Select(p => new ProductoResponseDto
                {
                    ProductoId = p.Id,
                    Codigo = p.Codigo,
                    Descripcion = p.Descripcion,
                    PrecioUnitario = p.Precio,
                    IVA = p.IVA,
                    Unidad = p.Unidad,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaModificacion = p.FechaModificacion
                })
                .FirstOrDefaultAsync();

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(producto);
        }

        ///<summary>
        ///Crea un nuevo producro
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductoResponseDto>> CrearProducto(CrearProductoDto dto)
        {
            //Validar con fluentValidation
            var validationResult = await _crerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = _tenantContext.GetTenantId();
            return Unauthorized(new { message = "Tenant no encontrado" });

            var existeCodigo = await _context.Productos
                .AnyAsync(p => p.TenantId == tenantId.Value && p.Codigo == dto.Codigo);
            if (existeCodigo)
                return Conflict(new { message = "Ya existe un porducto con ese codigo" });

            var producto = new Producto
            {
                TenantId = tenantId.Value,
                Codigo = dto.Codigo,
                Descripcion = dto.Descripcion,
                Precio = dto.PrecioUnitario,
                IVA = dto.IVA,
                Unidad = dto.Unidad,
                Activo = dto.Activo,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var response = new ProductoResponseDto
            {
                ProductoId = producto.Id,
                Codigo = producto.Codigo,
                Descripcion = producto.Descripcion,
                PrecioUnitario = producto.Precio,
                IVA = producto.IVA,
                Unidad = producto.Unidad,
                Activo = producto.Activo,
                FechaCreacion = producto.FechaCreacion,
                FechaModificacion = producto.FechaModificacion
            };

            return CreatedAtAction(nameof(GetProducto), new {id= producto.Id}, response);
        }

        ///<summary>
        ///Actualizar un producto
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductoResponseDto>> ActualizarProducto(int id, ActualizarProductoDto dto)
        {
            //Validar con FluentValidation
            var validationResult = await _actualizarValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no identificado" });

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId.Value);

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.PrecioUnitario;
            producto.IVA = dto.IVA;
            producto.Unidad = dto.Unidad;
            producto.Activo = dto.Activo;
            producto.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new ProductoResponseDto
            {
                ProductoId = producto.Id,
                Codigo = producto.Codigo,
                Descripcion = dto.Descripcion,
                PrecioUnitario = dto.PrecioUnitario,
                IVA = dto.IVA,
                Unidad = dto.Unidad,
                Activo = dto.Activo,
                FechaCreacion = dto.FechaCreacion,
                FechaModificacion = producto.FechaModificacion
            };

            return Ok(response);
        }

        ///<summary>
        ///Elimina un producto
        /// </summary>
        public async Task<ActionResult> EliminarProducto(int id)
        {
            var tenantId = _tenantContext.GetTenantId();
            if (tenantId == null)
                return Unauthorized(new { message = "Tenant no encontrado" });

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p=> p.Id == id && p.TenantId == tenantId.Value);

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            var tieneLineas = await _context.LineasFacturas
                .AnyAsync(l => l.Id == producto.Id);

            if (tieneLineas)
                return BadRequest(new { message = "No se puede eliminar el producto porque esta en uso en facturas" });

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
