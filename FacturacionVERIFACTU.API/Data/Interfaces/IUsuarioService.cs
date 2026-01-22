using FacturacionVERIFACTU.API.DTOs;

namespace FacturacionVERIFACTU.API.Services;

public interface IUsuarioService
{
    Task<List<UsuarioResponseDto>> ObtenerTodosAsync(int tenantId);
    Task<UsuarioResponseDto?> ObtenerPorIdAsync(int tenantId, int id);
    Task<UsuarioResponseDto> CrearUsuarioAsync(int tenantId, CreateUsuarioDto dto);
    Task<UsuarioResponseDto> ActualizarUsuarioAsync(int tenantId, int id, UpdateUsuarioDto dto, int currentUserId);
    Task<UsuarioResponseDto> CambiarEstadoAsync(int tenantId, int id, bool activo, int currentUserId);
    Task<bool> EliminarAsync(int tenantId, int id, int currentUserId);
    Task CambiarPasswordAsync(int userId, CambiarPasswordDto dto);
    Task<ResetPasswordResponseDto> ResetearPasswordAsync(int tenantId, int id);
}
