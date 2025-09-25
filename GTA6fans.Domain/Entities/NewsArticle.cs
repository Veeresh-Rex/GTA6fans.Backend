using MongoDB.Bson.Serialization.Attributes;

namespace GTA6fans.Domain.Entities;

[BsonCollection("news_articles")]
public class NewsArticle : BaseEntity
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("snippet")]
    public string Snippet { get; set; } = string.Empty;

    [BsonElement("author")]
    public string Author { get; set; } = string.Empty;

    [BsonElement("externalUrl")]
    public string ExternalUrl { get; set; } = string.Empty;

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [BsonElement("imageHint")]
    public string? ImageHint { get; set; }

    [BsonElement("likes")]
    public int Likes { get; set; } = 0;

    [BsonElement("publishedAt")]
    public DateTime PublishedAt { get; set; }
}