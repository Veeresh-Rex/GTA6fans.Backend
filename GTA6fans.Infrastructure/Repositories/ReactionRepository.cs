using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;

namespace GTA6fans.Infrastructure.Repositories;

public class ReactionRepository : GenericRepository<Reaction>, IReactionRepository
{
    public ReactionRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reaction>> GetByDocumentIdAsync(string documentId, DocumentType documentType)
    {
        return await _collection.Find(x => x.DocumentId == documentId && x.DocumentType == documentType && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reaction>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(x => x.UserId == userId && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Reaction?> GetUserReactionAsync(string documentId, DocumentType documentType, string userId)
    {
        return await _collection.Find(x => x.DocumentId == documentId && x.DocumentType == documentType && x.UserId == userId && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<string, int>> GetReactionCountsAsync(string documentId, DocumentType documentType)
    {
        var reactions = await _collection.Find(x => x.DocumentId == documentId && x.DocumentType == documentType && !x.IsDeleted)
            .ToListAsync();

        return reactions.GroupBy(r => r.Emoji)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<bool> RemoveUserReactionAsync(string documentId, DocumentType documentType, string userId)
    {
        var filter = Builders<Reaction>.Filter.And(
            Builders<Reaction>.Filter.Eq(x => x.DocumentId, documentId),
            Builders<Reaction>.Filter.Eq(x => x.DocumentType, documentType),
            Builders<Reaction>.Filter.Eq(x => x.UserId, userId),
            Builders<Reaction>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<Reaction>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpsertReactionAsync(string documentId, DocumentType documentType, string userId, string emoji)
    {
        var filter = Builders<Reaction>.Filter.And(
            Builders<Reaction>.Filter.Eq(x => x.DocumentId, documentId),
            Builders<Reaction>.Filter.Eq(x => x.DocumentType, documentType),
            Builders<Reaction>.Filter.Eq(x => x.UserId, userId)
        );

        var update = Builders<Reaction>.Update
            .Set(x => x.Emoji, emoji)
            .Set(x => x.IsDeleted, false)
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .SetOnInsert(x => x.DocumentId, documentId)
            .SetOnInsert(x => x.DocumentType, documentType)
            .SetOnInsert(x => x.UserId, userId)
            .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        return result.ModifiedCount > 0 || result.UpsertedId != null;
    }
}