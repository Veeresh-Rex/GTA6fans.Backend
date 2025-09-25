using GTA6fans.Domain.Entities;

namespace GTA6fans.Domain.Interfaces;

public interface IForumReplyRepository : IGenericRepository<ForumReply>
{
    Task<IEnumerable<ForumReply>> GetByTopicIdAsync(string topicId);
    Task<IEnumerable<ForumReply>> GetByAuthorIdAsync(string authorId);
    Task<IEnumerable<ForumReply>> GetRepliesByParentIdAsync(string parentId);
    Task<long> CountByTopicIdAsync(string topicId);
    Task<ForumReply?> GetLatestReplyByTopicIdAsync(string topicId);
}