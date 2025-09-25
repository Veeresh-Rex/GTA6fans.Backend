using MongoDB.Bson.Serialization.Attributes;
using GTA6fans.Domain.Enums;
using MongoDB.Bson;

namespace GTA6fans.Domain.Entities;

[BsonCollection("users")]
public class User : BaseEntity
{
    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public UserStatus Status { get; set; } = UserStatus.Active;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public UserType Type { get; set; } = UserType.User;
}
