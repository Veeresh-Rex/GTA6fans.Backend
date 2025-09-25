using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;

namespace GTA6fans.Infrastructure.Repositories;

public class ForumTopicRepository : GenericRepository<ForumTopic>, IForumTopicRepository
{
    public ForumTopicRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ForumTopic>> GetByCategoryAsync(string category)
    {
        return await _collection.Find(x => x.Category == category && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForumTopic>> GetByAuthorIdAsync(string authorId)
    {
        return await _collection.Find(x => x.AuthorId == authorId && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForumTopic>> GetMostPopularAsync(int limit = 10)
    {
        return await _collection.Find(x => !x.IsDeleted)
            .SortByDescending(x => x.ReplyCount)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForumTopic>> GetRecentAsync(int limit = 10)
    {
        return await _collection.Find(x => !x.IsDeleted)
            .SortByDescending(x => x.LastActivity != null ? x.LastActivity.Timestamp : x.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<bool> IncrementReplyCountAsync(string topicId)
    {
        var filter = Builders<ForumTopic>.Filter.Eq(x => x.Id, topicId);
        var update = Builders<ForumTopic>.Update
            .Inc(x => x.ReplyCount, 1)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateByReplyAsync(string topicId, LastActivity lastActivity)
    {
        var filter = Builders<ForumTopic>.Filter.Eq(x => x.Id, topicId);
        var update = Builders<ForumTopic>.Update
            .Set(x => x.LastActivity, lastActivity)
            .Inc(x => x.ReplyCount, 1)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }
}