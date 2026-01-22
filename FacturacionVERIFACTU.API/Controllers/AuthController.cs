using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Services;
using FacturacionVERIFACTU.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionVERIFACTU.API.Data.Interfaces;


namespace FacturacionVERIFACTU.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController :ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHashService _hashService;
        private readonly IJwtService _jwtService;
        private readonly ITenantInitializationService _tenantInitService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IHashService hasService,
            IJwtService jwtService, 
            ITenantInitializationService tenantIntiService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _hashService = hasService;
            _jwtService = jwtService;
            _tenantInitService = tenantIntiService;
            _logger = logger;
        }

        ///<summary>
        ///POST /api/auth/register - Registra nuevo usuario y empresa
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Verificar email único
                var emailExists = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
                if (emailExists)
                {
                    return BadRequest(new { mensaje = "El email ya está registrado" });
                }

                // 2. Verificar NIF único
                var nifExists = await _context.Tenants.AnyAsync(t => t.NIF == request.NIF);
                if (nifExists)
                {
                    return BadRequest(new { mensaje = "El NIF ya está registrado" });
                }

                // 3. Crear Tenant
                var tenant = new Tenant
                {
                    Nombre = request.NombreEmpresa,
                    NIF = request.NIF,
                    Activo = true,
                    FechaAlta = DateTime.UtcNow
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                // 4. Crear Usuario Admin
                var usuario = new Usuario
                {
                    Email = request.Email,
                    PasswordHash = _hashService.Hash(request.Password),
                    Nombre = request.NombreCompleto,
                    Rol = "Admin",
                    Activo = true,
                    TenantId = tenant.Id,
                    FechaCreaccion = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();


                // 5. ✨ INICIALIZAR TENANT (NUEVO)
                await _tenantInitService.IncicializarTenantAsync(tenant.Id);

                // 6. Generar tokens
                var accessToken = await _jwtService.GenerateAccessToken(
                    usuario.Id, usuario.Email, tenant.Id, usuario.Rol);
                var refreshToken =  _jwtService.GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UsuarioId = usuario.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    Revoked = false
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Empresa {Empresa} y usuario {Email} registrados correctamente",
                    tenant.Nombre, usuario.Email);

                return Ok(new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    User = new UserInfo
                    {
                        UserId = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.Nombre,
                        TenantId = tenant.Id,
                        NombreEmpresa = tenant.Nombre,
                        Role = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en registro de empresa");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
        /// <summary>
        /// POST /api/auth/login - Inicia sesión
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            // Buscar usuario con tenant
            var usuario = await _context.Usuarios
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Verificar contraseña
            if (!_hashService.VerifyPassword(request.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Verificar que usuario y tenant estén activos
            if (!usuario.Activo || !usuario.Tenant.Activo)
            {
                return Unauthorized(new { message = "Usuario o empresa inactivos" });
            }

            // Generar tokens
            var accessToken = await _jwtService.GenerateAccessToken(
                usuario.Id,
                usuario.Email,
                usuario.TenantId,
                usuario.Role
            );

            var refreshToken = _jwtService.GenerateRefreshToken();

            // Revocar tokens anteriores del usuario
            var oldTokens = await _context.RefreshTokens
                .Where(rt => rt.UsuarioId == usuario.Id && !rt.Revoked)
                .ToListAsync();

            foreach (var token in oldTokens)
            {
                token.Revoked = true;
            }

            // Guardar nuevo refresh token
            var tokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UsuarioId = usuario.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                User = new UserInfo
                {
                    UserId = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    TenantId = usuario.TenantId,
                    NombreEmpresa = usuario.Tenant.NombreEmpresa,
                    Role = usuario.Role
                }
            });
        }

        /// <summary>
        /// POST /api/auth/refresh-token - Refresca access token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            // Buscar refresh token válido
            var tokenEntity = await _context.RefreshTokens
                .Include(rt => rt.Usuario)
                    .ThenInclude(u => u.Tenant)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == request.RefreshToken &&
                    !rt.Revoked &&
                    rt.ExpiresAt > DateTime.UtcNow
                );

            if (tokenEntity == null)
            {
                return Unauthorized(new { message = "Refresh token inválido o expirado" });
            }

            var usuario = tokenEntity.Usuario;

            // Verificar que usuario y tenant estén activos
            if (!usuario.Activo || !usuario.Tenant.Activo)
            {
                return Unauthorized(new { message = "Usuario o empresa inactivos" });
            }

            // Generar nuevos tokens
            var newAccessToken = await _jwtService.GenerateAccessToken(
                usuario.Id,
                usuario.Email,
                usuario.TenantId,
                usuario.Role
            );

            var newRefreshToken =  _jwtService.GenerateRefreshToken();

            // Revocar token anterior
            tokenEntity.Revoked = true;

            // Guardar nuevo refresh token
            var newTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UsuarioId = usuario.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                User = new UserInfo
                {
                    UserId = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    TenantId = usuario.TenantId,
                    NombreEmpresa = usuario.Tenant.NombreEmpresa,
                    Role = usuario.Role
                }
            });
        }
    }
}
  
