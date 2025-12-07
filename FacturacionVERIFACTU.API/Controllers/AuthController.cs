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

        public AuthController(
            ApplicationDbContext context,
            IHashService hasService,
            IJwtService jwtService)
        {
            _context = context;
            _hashService = hasService;
            _jwtService = jwtService;
        }

        ///<summary>
        ///POST /api/auth/register - Registra nuevo usuario y empresa
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            //validar email unico
            if (await _context.Usuarios.AnyAsync(u=> u.Email == request.Email))
            {
                return BadRequest(new { message = "El email ya esta registrado" });
            }

            //Validar NIF unico
            if (await _context.Tenants.AnyAsync(t => t.NIF == request.NIF))
            {
                return BadRequest(new { message = "El NIF ya esta registrado" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //1. Crear Tenant
                var tenant = new Tenant
                {
                    NombreEmpresa = request.NombreEmpresa,
                    NIF = request.NIF,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                //2.Crear usuario
                var usuario = new Usuario
                {
                    Email = request.Email,
                    PasswordHash = _hashService.HashPassword(request.Password),
                    NombreCompleto = request.NombreCompleto,
                    Rol = "Admin", // primer usuario es admin
                    TenantId = tenant.Id,
                    Activo = true,
                    FechaCreaccion = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                //3.Generar tokens
                var accessToken = _jwtService.GenerateAccessToken(
                    usuario.Id,
                    usuario.Email,
                    tenant.Id,
                    usuario.Rol
                 );

                var refreshToken = _jwtService.GenerateRefreshToken();

                //4.Guardar refreshtoken
                var tokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UsuarioId = usuario.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(tokenEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                //5. Retornar respuesta
                return Ok(new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt= DateTime.UtcNow.AddMinutes(30),
                    User = new UserInfo
                    {
                        UserId = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        TenantId = tenant.Id,
                        NombreEmpresa = tenant.NombreEmpresa,
                        Role = usuario.Rol
                    }   
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new {message ="Error al registrar el usuario", error = ex.Message});
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
            if (_hashService.VerifyPassword(request.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Verificar que usuario y tenant estén activos
            if (!usuario.Activo || !usuario.Tenant.Activo)
            {
                return Unauthorized(new { message = "Usuario o empresa inactivos" });
            }

            // Generar tokens
            var accessToken = _jwtService.GenerateAccessToken(
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
            var newAccessToken = _jwtService.GenerateAccessToken(
                usuario.Id,
                usuario.Email,
                usuario.TenantId,
                usuario.Role
            );

            var newRefreshToken = _jwtService.GenerateRefreshToken();

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
  
