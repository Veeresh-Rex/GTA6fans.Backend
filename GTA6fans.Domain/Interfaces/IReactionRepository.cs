using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Domain.Interfaces;

public interface IReactionRepository : IGenericRepository<Reaction>
{
    Task<IEnumerable<Reaction>> GetByDocumentIdAsync(string documentId, DocumentType documentType);
    Task<IEnumerable<Reaction>> GetByUserIdAsync(string userId);
    Task<Reaction?> GetUserReactionAsync(string documentId, DocumentType documentType, string userId);
    Task<Dictionary<string, int>> GetReactionCountsAsync(string documentId, DocumentType documentType);
    Task<bool> RemoveUserReactionAsync(string documentId, DocumentType documentType, string userId);
    Task<bool> UpsertReactionAsync(string documentId, DocumentType documentType, string userId, string emoji);
}