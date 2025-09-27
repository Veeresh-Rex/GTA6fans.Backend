using GTA6fans.Domain.Enums;

namespace GTA6fans.Application.DTOs;

public class ReactionRequestDto
{
    public string Emoji { get; set; }
    public string DocumentId { get; set; }
    public DocumentType DocumentType { get; set; }
    public bool IsRevoking { get;set; }
}
