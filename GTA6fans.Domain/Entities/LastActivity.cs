using MongoDB.Bson.Serialization.Attributes;

namespace GTA6fans.Domain.Entities;

public class LastActivity
{
    [BsonElement("authorId")]
    public string AuthorId { get; set; } = string.Empty;

    [BsonElement("authorName")]
    public string AuthorName { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}