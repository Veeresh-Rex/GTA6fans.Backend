namespace GTA6fans.Application.DTOs;

public class NewsDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageHint { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
    public int Likes { get; set; }
}

public class CreateNewsArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageHint { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
}