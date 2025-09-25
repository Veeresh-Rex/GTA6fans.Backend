namespace GTA6fans.Application.DTOs;

public class CreateReplyRequest
{
    public string TopicId { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}