using GTA6fans.Application.DTOs;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Models;

namespace GTA6fans.Application.Interfaces;

public interface IForumService
{
    Task<CreateForumResponseDTO> CreateTopicAsync(CreateForumRequestDTO requestDTO, string authorId);
    Task<ForumResponseDTO> GetForumbySlug(string slug);
    Task<PagedResult<ForumResponseDTO>> GetForumList(int page, int pageSize, bool? sortByPopularity);
    Task AddReplyAsync(CreateReplyRequest request);
}