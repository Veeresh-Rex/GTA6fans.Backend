using MongoDB.Bson.Serialization.Attributes;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Domain.Entities;

[BsonCollection("videos")]
public class Video : BaseEntity
{
    [BsonElement("youtubeId")]
    public string YoutubeId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("submittedBy")]
    public string SubmittedBy { get; set; } = string.Empty;

    [BsonElement("author")]
    public string Author { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public VideoStatus Status { get; set; } = VideoStatus.Pending;
}