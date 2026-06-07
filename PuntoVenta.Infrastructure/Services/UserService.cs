using Mapster;
using PuntoVenta.Application.DTOs.User;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Application.Interfaces.Services;
using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
        => (await _userRepository.GetAllAsync()).Adapt<IEnumerable<UserDto>>();

    public async Task<UserDto?> GetByIdAsync(int userId)
        => (await _userRepository.GetByIdAsync(userId))?.Adapt<UserDto>();

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingUser is not null)
            throw new InvalidOperationException($"El usuario '{dto.Username}' ya existe.");

        var role = await _roleRepository.GetByIdAsync(dto.RoleId)
            ?? throw new KeyNotFoundException($"Rol {dto.RoleId} no encontrado.");

        var user = User.Create(
            dto.Username,
            SecurityPasswordHasher.HashPassword(dto.Password),
            dto.FullName,
            role.RoleId,
            dto.Email);

        var savedUser = await _userRepository.AddAsync(user);
        return savedUser.Adapt<UserDto>();
    }

    public async Task<bool> UpdateAsync(int userId, UpdateUserDto dto, string? modifiedBy = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return false;

        var role = await _roleRepository.GetByIdAsync(dto.RoleId)
            ?? throw new KeyNotFoundException($"Rol {dto.RoleId} no encontrado.");

        // Verificar si se está intentando cambiar el rol del único admin activo a uno que no es admin
        if (user.Role != null && user.Role.Name.Equals(PuntoVenta.Application.Constants.AppRoles.Admin, System.StringComparison.OrdinalIgnoreCase) &&
            !role.Name.Equals(PuntoVenta.Application.Constants.AppRoles.Admin, System.StringComparison.OrdinalIgnoreCase))
        {
            var allUsers = await _userRepository.GetAllAsync();
            var activeAdminsCount = allUsers.Count(u => u.IsActive && u.Role != null && u.Role.Name.Equals(PuntoVenta.Application.Constants.AppRoles.Admin, System.StringComparison.OrdinalIgnoreCase));
            if (activeAdminsCount <= 1)
            {
                throw new System.InvalidOperationException("No se puede cambiar el rol del único administrador activo del sistema.");
            }
        }

        user.UpdateProfile(dto.FullName, dto.Email, modifiedBy);
        user.ChangeRole(role.RoleId, modifiedBy);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangeUserPasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return false;

        user.ChangePasswordHash(SecurityPasswordHasher.HashPassword(dto.Password));
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public Task<bool> ActivateAsync(int userId) => _userRepository.ActivateAsync(userId);

    public async Task<bool> DeactivateAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return false;

        if (user.Role != null && user.Role.Name.Equals(PuntoVenta.Application.Constants.AppRoles.Admin, System.StringComparison.OrdinalIgnoreCase))
        {
            var allUsers = await _userRepository.GetAllAsync();
            var activeAdminsCount = allUsers.Count(u => u.IsActive && u.Role != null && u.Role.Name.Equals(PuntoVenta.Application.Constants.AppRoles.Admin, System.StringComparison.OrdinalIgnoreCase));
            if (activeAdminsCount <= 1)
            {
                throw new System.InvalidOperationException("No se puede desactivar el único administrador activo del sistema.");
            }
        }

        return await _userRepository.DeactivateAsync(userId);
    }

    public async Task<bool> UnlockAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return false;

        user.UnlockUser();
        await _userRepository.UpdateAsync(user);
        return true;
    }
}