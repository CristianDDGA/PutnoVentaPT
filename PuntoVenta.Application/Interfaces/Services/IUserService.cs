using PuntoVenta.Application.DTOs.User;

namespace PuntoVenta.Application.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int userId);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<bool> UpdateAsync(int userId, UpdateUserDto dto, string? modifiedBy = null);
    Task<bool> ChangePasswordAsync(int userId, ChangeUserPasswordDto dto);
    Task<bool> ActivateAsync(int userId);
    Task<bool> DeactivateAsync(int userId);
    Task<bool> UnlockAsync(int userId);
}