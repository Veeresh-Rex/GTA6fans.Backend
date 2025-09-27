using GTA6fans.Application.DTOs;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Models;

namespace GTA6fans.Application.Interfaces;

public interface INewsService
{
    Task<NewsArticle> CreateNewsArticleAsync(CreateNewsArticleRequest request);
    Task<PagedResult<NewsDto>> GetNewsList(string? userId, string? query, int page, int pageSize);
}
