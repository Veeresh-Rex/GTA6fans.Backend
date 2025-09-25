using GTA6fans.Domain.Entities;

namespace GTA6fans.Domain.Interfaces;

public interface IForumTopicRepository : IGenericRepository<ForumTopic>
{
    Task<IEnumerable<ForumTopic>> GetByCategoryAsync(string category);
    Task<IEnumerable<ForumTopic>> GetByAuthorIdAsync(string authorId);
    Task<IEnumerable<ForumTopic>> GetMostPopularAsync(int limit = 10);
    Task<IEnumerable<ForumTopic>> GetRecentAsync(int limit = 10);
    Task<bool> IncrementReplyCountAsync(string topicId);
    Task<bool> UpdateByReplyAsync(string topicId, LastActivity lastActivity);
}