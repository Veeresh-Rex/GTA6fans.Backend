using MongoDB.Bson.Serialization.Attributes;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Domain.Entities;

[BsonCollection("reactions")]
public class Reaction : BaseEntity
{
    [BsonElement("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [BsonElement("documentType")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DocumentType DocumentType { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("emoji")]
    public string Emoji { get; set; } = string.Empty;
}