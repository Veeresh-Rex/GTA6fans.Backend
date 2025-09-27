using System.Text.Json.Serialization;

namespace GTA6fans.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<DocumentType>))]
public enum DocumentType
{
    Topic,
    Comment
}