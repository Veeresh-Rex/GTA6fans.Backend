using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByDisplayNameAsync(string displayName);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsDisplayNameAvailableAsync(string displayName);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<IEnumerable<User>> GetByStatusAsync(UserStatus status);
    Task<bool> UpdateUserStatusAsync(string userId, UserStatus status);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetBannedUsersAsync();
    Task<IEnumerable<User>> GetSuspendedUsersAsync();
}
