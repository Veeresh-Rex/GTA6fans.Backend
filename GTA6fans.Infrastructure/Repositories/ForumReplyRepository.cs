using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;

namespace GTA6fans.Infrastructure.Repositories;

public class ForumReplyRepository : GenericRepository<ForumReply>, IForumReplyRepository
{
    public ForumReplyRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ForumReply>> GetByTopicIdAsync(string topicId)
    {
        return await _collection.Find(x => x.TopicId == topicId && !x.IsDeleted)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForumReply>> GetByAuthorIdAsync(string authorId)
    {
        return await _collection.Find(x => x.AuthorId == authorId && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForumReply>> GetRepliesByParentIdAsync(string parentId)
    {
        return await _collection.Find(x => x.ParentId == parentId && !x.IsDeleted)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<long> CountByTopicIdAsync(string topicId)
    {
        return await _collection.CountDocumentsAsync(x => x.TopicId == topicId && !x.IsDeleted);
    }

    public async Task<ForumReply?> GetLatestReplyByTopicIdAsync(string topicId)
    {
        return await _collection.Find(x => x.TopicId == topicId && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}