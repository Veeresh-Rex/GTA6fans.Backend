using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;

namespace GTA6fans.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByDisplayNameAsync(string displayName)
    {
        return await _collection.Find(x => x.DisplayName == displayName && !x.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(x => x.Email == email && !x.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<bool> IsDisplayNameAvailableAsync(string displayName)
    {
        var user = await _collection.Find(x => x.DisplayName == displayName && !x.IsDeleted).FirstOrDefaultAsync();
        return user == null;
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var user = await _collection.Find(x => x.Email == email && !x.IsDeleted).FirstOrDefaultAsync();
        return user == null;
    }

    public async Task<IEnumerable<User>> GetByStatusAsync(UserStatus status)
    {
        return await _collection.Find(x => x.Status == status && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateUserStatusAsync(string userId, UserStatus status)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Id, userId);
        var update = Builders<User>.Update
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _collection.Find(x => x.Status == UserStatus.Active && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetBannedUsersAsync()
    {
        return await _collection.Find(x => x.Status == UserStatus.Banned && !x.IsDeleted)
            .SortByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetSuspendedUsersAsync()
    {
        return await _collection.Find(x => x.Status == UserStatus.Suspended && !x.IsDeleted)
            .SortByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }
}
