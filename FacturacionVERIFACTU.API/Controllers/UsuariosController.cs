using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionVERIFACTU.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            IUsuarioService usuarioService,
            ITenantContext tenantContext,
            ILogger<UsuariosController> logger)
        {
            _usuarioService = usuarioService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios del tenant actual
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<UsuarioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<UsuarioResponseDto>>> ObtenerTodos()
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                var usuarios = await _usuarioService.ObtenerTodosAsync(tenantId.Value);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { mensaje = "Error al obtener usuarios" });
            }
        }

        /// <summary>
        /// Obtiene un usuario por Id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                var usuario = await _usuarioService.ObtenerPorIdAsync(tenantId.Value, id);

                if (usuario == null)
                {
                    return NotFound(new { mensaje = $"Usuario {id} no encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {Id}", id);
                return StatusCode(500, new { mensaje = "Error al obtener el usuario" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario (solo Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioResponseDto>> Crear([FromBody] CreateUsuarioDto dto)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.CrearUsuarioAsync(tenantId.Value, dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = usuario.Id },
                    usuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear usuario");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { mensaje = "Error al crear el usuario" });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente (solo Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioResponseDto>> Actualizar(
            int id,
            [FromBody] UpdateUsuarioDto dto)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Obtener usuario actual para validaciones
                var currentUserId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");

                var usuario = await _usuarioService.ActualizarUsuarioAsync(
                    tenantId.Value,
                    id,
                    dto,
                    currentUserId);

                return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar usuario {Id}", id);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
                return StatusCode(500, new { mensaje = "Error al actualizar usuario" });
            }
        }

        /// <summary>
        /// Cambia el estado de un usuario (Activo/Inactivo)
        /// Solo Admin puede desactivar usuarios
        /// </summary>
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioResponseDto>> CambiarEstado(
            int id,
            [FromBody] CambiarEstadoUsuarioDto dto)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");

                var usuario = await _usuarioService.CambiarEstadoAsync(
                    tenantId.Value,
                    id,
                    dto.Activo,
                    currentUserId);

                return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al cambiar estado del usuario {Id}", id);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del usuario {Id}", id);
                return StatusCode(500, new { mensaje = "Error al cambiar estado" });
            }
        }

        /// <summary>
        /// Elimina (desactiva) un usuario
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                var currentUserId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");

                var eliminado = await _usuarioService.EliminarAsync(tenantId.Value, id, currentUserId);

                if (!eliminado)
                {
                    return NotFound(new { mensaje = $"Usuario {id} no encontrado" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al eliminar usuario {Id}", id);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
                return StatusCode(500, new { mensaje = "Error al eliminar el usuario" });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario actual
        /// </summary>
        [HttpPost("cambiar-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");

                await _usuarioService.CambiarPasswordAsync(userId, dto);

                return Ok(new { mensaje = "Contraseña actualizada correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al cambiar contraseña");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new { mensaje = "Error al cambiar contraseña" });
            }
        }

        /// <summary>
        /// Resetea la contraseña de un usuario (solo Admin)
        /// Genera una nueva contraseña temporal
        /// </summary>
        [HttpPost("{id}/resetear-password")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResetPasswordResponseDto>> ResetearPassword(int id)
        {
            try
            {
                var tenantId = _tenantContext.GetTenantId();
                if (tenantId == null || tenantId == 0)
                {
                    return Unauthorized(new { mensaje = "Tenant no identificado" });
                }

                var resultado = await _usuarioService.ResetearPasswordAsync(tenantId.Value, id);

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al resetear contraseña del usuario {Id}", id);
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña del usuario {Id}", id);
                return StatusCode(500, new { mensaje = "Error al resetear contraseña" });
            }
        }
    }
}