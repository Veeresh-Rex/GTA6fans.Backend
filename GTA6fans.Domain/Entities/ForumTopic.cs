using MongoDB.Bson.Serialization.Attributes;

namespace GTA6fans.Domain.Entities;

[BsonCollection("forum_topics")]
public class ForumTopic : BaseEntity
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("authorId")]
    public string AuthorId { get; set; } = string.Empty;

    [BsonElement("authorName")]
    public string AuthorName { get; set; } = string.Empty;

    [BsonElement("images")]
    public List<string> Images { get; set; } = new();

    [BsonElement("replyCount")]
    public int ReplyCount { get; set; } = 0;

    [BsonElement("lastActivity")]
    public LastActivity? LastActivity { get; set; }
}