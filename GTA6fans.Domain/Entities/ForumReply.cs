using MongoDB.Bson.Serialization.Attributes;

namespace GTA6fans.Domain.Entities;

[BsonCollection("forum_replies")]
public class ForumReply : BaseEntity
{
    [BsonElement("topicId")]
    public string TopicId { get; set; } = string.Empty;

    [BsonElement("parentId")]
    public string? ParentId { get; set; }

    [BsonElement("authorId")]
    public string AuthorId { get; set; } = string.Empty;

    [BsonElement("authorName")]
    public string AuthorName { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;
}