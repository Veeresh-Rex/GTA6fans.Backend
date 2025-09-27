using GTA6fans.Application.DTOs;
using GTA6fans.Domain.Models;

namespace GTA6fans.Application.Interfaces;

public interface IForumService
{
    Task<CreateForumResponseDTO> CreateTopicAsync(CreateForumRequestDTO requestDTO, string authorId);
    Task<ForumResponseDTO> GetForumbySlug(string slug, string? userId);
    Task<PagedResult<ForumResponseDTO>> GetForumList(string? userId, string? query, string? scope, string? category, int page, int pageSize, bool? sortByPopularity);
    Task<Reply> AddReplyAsync(CreateReplyRequest request);
    Task SubmitReactionAsync(ReactionRequestDto request, string userId);
}