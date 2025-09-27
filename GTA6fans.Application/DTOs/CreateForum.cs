using GTA6fans.Domain.Entities;

namespace GTA6fans.Application.DTOs;

public class CreateForumRequestDTO
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
}


public class CreateForumResponseDTO
{
    public string Slug { get; set; }
}


public class ForumResponseDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public Author Author { get; set; }
    public string Timestamp { get; set; }
    public LastPost LastPost { get; set; }
    public List<Reply> Replies { get; set; } = new();
    public int RepliesCount { get; set; }
    public List<string>? Images { get; set; }
    public string Slug { get; set; }
    public Dictionary<string, int> Reactions { get; set; } = new();
    public string? MyReaction { get; set; } = null;
}

public class Author
{
    public string Name { get; set; }
}

public class LastPost
{
    public string Author { get; set; }
    public string Timestamp { get; set; }
}

public class Reply
{
    public string Id { get; set; }

    public Author Author { get; set; }

    public string Timestamp { get; set; }

    public string Content { get; set; }

    public List<Reply>? Replies { get; set; }

    public Dictionary<string, int> Reactions { get; set; } = new();
    public string? MyReaction { get; set; } = null;
}

public class Reaction
{
    public string Id { get; set; }
    public string Emoji { get; set; }
    public string UserId { get; set; }
}