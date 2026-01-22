using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FacturacionVERIFACTU.API.Services;

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly IHashService _hashService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        ApplicationDbContext context,
        IHashService hashService,
        ILogger<UsuarioService> logger)
    {
        _context = context;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<List<UsuarioResponseDto>> ObtenerTodosAsync(int tenantId)
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Tenant)
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                NombreCompleto = u.NombreCompleto,
                Role = u.Role,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion,
                TenantId = u.TenantId,
                NombreEmpresa = u.Tenant.NombreEmpresa
            })
            .ToListAsync();

        return usuarios;
    }

    public async Task<UsuarioResponseDto?> ObtenerPorIdAsync(int tenantId, int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Tenant)
            .Where(u => u.Id == id && u.TenantId == tenantId)
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                NombreCompleto = u.NombreCompleto,
                Role = u.Role,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion,
                TenantId = u.TenantId,
                NombreEmpresa = u.Tenant.NombreEmpresa
            })
            .FirstOrDefaultAsync();

        return usuario;
    }

    public async Task<UsuarioResponseDto> CrearUsuarioAsync(int tenantId, CreateUsuarioDto dto)
    {
        // Validar email único
        var emailExists = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExists)
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        // Validar que el tenant exista
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant no encontrado");
        }

        var usuario = new Usuario
        {
            Email = dto.Email,
            PasswordHash = _hashService.Hash(dto.Password),
            NombreCompleto = dto.NombreCompleto,
            Role = dto.Role,
            Activo = dto.Activo,
            TenantId = tenantId,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Email} creado en tenant {TenantId}", usuario.Email, tenantId);

        return new UsuarioResponseDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Role = usuario.Role,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            TenantId = usuario.TenantId,
            NombreEmpresa = tenant.NombreEmpresa
        };
    }

    public async Task<UsuarioResponseDto> ActualizarUsuarioAsync(
        int tenantId,
        int id,
        UpdateUsuarioDto dto,
        int currentUserId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // No permitir que un usuario se desactive a sí mismo
        if (usuario.Id == currentUserId && !dto.Activo)
        {
            throw new InvalidOperationException("No puedes desactivarte a ti mismo");
        }

        usuario.NombreCompleto = dto.NombreCompleto;
        usuario.Role = dto.Role;
        usuario.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Id} actualizado en tenant {TenantId}", id, tenantId);

        return new UsuarioResponseDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Role = usuario.Role,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            TenantId = usuario.TenantId,
            NombreEmpresa = usuario.Tenant.NombreEmpresa
        };
    }

    public async Task<UsuarioResponseDto> CambiarEstadoAsync(
        int tenantId,
        int id,
        bool activo,
        int currentUserId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // No permitir que un usuario se desactive a sí mismo
        if (usuario.Id == currentUserId && !activo)
        {
            throw new InvalidOperationException("No puedes desactivarte a ti mismo");
        }

        usuario.Activo = activo;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Id} cambió estado a {Activo} en tenant {TenantId}",
            id, activo, tenantId);

        return new UsuarioResponseDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Role = usuario.Role,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            TenantId = usuario.TenantId,
            NombreEmpresa = usuario.Tenant.NombreEmpresa
        };
    }

    public async Task<bool> EliminarAsync(int tenantId, int id, int currentUserId)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

        if (usuario == null)
        {
            return false;
        }

        // No permitir que un usuario se elimine a sí mismo
        if (usuario.Id == currentUserId)
        {
            throw new InvalidOperationException("No puedes eliminarte a ti mismo");
        }

        // Eliminación lógica (desactivar)
        usuario.Activo = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Id} eliminado (desactivado) en tenant {TenantId}", id, tenantId);

        return true;
    }

    public async Task CambiarPasswordAsync(int userId, CambiarPasswordDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(userId);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // Verificar contraseña actual
        if (!_hashService.Verify(dto.CurrentPassword, usuario.PasswordHash))
        {
            throw new InvalidOperationException("Contraseña actual incorrecta");
        }

        // Actualizar contraseña
        usuario.PasswordHash = _hashService.Hash(dto.NewPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Id} cambió su contraseña", userId);
    }

    public async Task<ResetPasswordResponseDto> ResetearPasswordAsync(int tenantId, int id)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // Generar contraseña temporal aleatoria
        var passwordTemporal = GenerarPasswordTemporal();

        // Actualizar contraseña
        usuario.PasswordHash = _hashService.Hash(passwordTemporal);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contraseña del usuario {Id} reseteada", id);

        return new ResetPasswordResponseDto
        {
            Email = usuario.Email,
            PasswordTemporal = passwordTemporal,
            Mensaje = "Contraseña reseteada correctamente. El usuario debe cambiarla en el próximo inicio de sesión."
        };
    }

    private string GenerarPasswordTemporal()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@$!%*?&";
        var random = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }

        var password = new char[12];
        for (int i = 0; i < 12; i++)
        {
            password[i] = chars[random[i] % chars.Length];
        }

        return new string(password);
    }
}