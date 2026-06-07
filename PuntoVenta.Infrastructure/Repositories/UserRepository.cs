using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces.Repositories;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _appDbContext.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .ToListAsync();

    public async Task<User?> GetByIdAsync(int userId)
        => await _appDbContext.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.UserId == userId);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _appDbContext.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Username == username);

    public async Task<User?> GetByEmailAsync(string email)
        => await _appDbContext.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == email);

    public async Task<User> AddAsync(User user)
    {
        await _appDbContext.Users.AddAsync(user);
        await _appDbContext.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        await _appDbContext.Users
            .Where(existingUser => existingUser.UserId == user.UserId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(existingUser => existingUser.FullName, user.FullName)
                .SetProperty(existingUser => existingUser.Email, user.Email)
                .SetProperty(existingUser => existingUser.RoleId, user.RoleId)
                .SetProperty(existingUser => existingUser.PasswordHash, user.PasswordHash)
                .SetProperty(existingUser => existingUser.IsActive, user.IsActive)
                .SetProperty(existingUser => existingUser.FailedLoginAttempts, user.FailedLoginAttempts)
                .SetProperty(existingUser => existingUser.IsLocked, user.IsLocked)
                .SetProperty(existingUser => existingUser.LastModifiedBy, user.LastModifiedBy)
                .SetProperty(existingUser => existingUser.LastModifiedAt, user.LastModifiedAt));
    }

    public async Task<bool> ActivateAsync(int userId)
        => await UpdateIsActiveAsync(userId, true);

    public async Task<bool> DeactivateAsync(int userId)
        => await UpdateIsActiveAsync(userId, false);

    private async Task<bool> UpdateIsActiveAsync(int userId, bool isActive)
        => await _appDbContext.Users.Where(user => user.UserId == userId)
            .ExecuteUpdateAsync(update => update.SetProperty(user => user.IsActive, isActive)) > 0;
}