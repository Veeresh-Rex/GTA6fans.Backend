using System.Linq.Expressions;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Domain.Models;

namespace GTA6fans.Application.Services;

public class NewsService : INewsService
{
    private readonly INewsArticleRepository _newsArticleRepository;

    public NewsService(INewsArticleRepository newsArticleRepository)
    {
        _newsArticleRepository = newsArticleRepository;
    }

    public Task<NewsArticle> CreateNewsArticleAsync(CreateNewsArticleRequest request)
    {
        var newsArticle = new NewsArticle
        {
            Title = request.Title,
            Snippet = request.Snippet,
            ImageUrl = request.ImageUrl,
            Author = request.Author,
            PublishedAt = request.PublishedAt,
            Likes = 0,
            ExternalUrl = request.ExternalUrl,
            ImageHint = request.ImageHint
        };

        return _newsArticleRepository.CreateAsync(newsArticle);
    }

    public async Task<PagedResult<NewsDto>> GetNewsList(string? userId, string? query, int page, int pageSize)
    {
        // Build filter condition
        Expression<Func<NewsArticle, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowered = query.ToLower();
            filter = x => x.Title.ToLower().Contains(lowered);
        }

        // Choose sorting
        Func<IQueryable<NewsArticle>, IOrderedQueryable<NewsArticle>> orderBy = x => x.OrderByDescending(e => e.CreatedAt);

        var pagedResult = await _newsArticleRepository.GetPagedAsync(page, pageSize, filter, orderBy);

        // Map to DTO
        var forumResponseDTOs = pagedResult.Items.Select(news => new NewsDto()
        {
            Id = news.Id,
            Title = news.Title,
            Snippet = news.Snippet,
            ImageUrl = news.ImageUrl,
            Timestamp = news.CreatedAt.ToString("O"),
            Author = news.Author,
            Likes = news.Likes,
            ExternalUrl = news.ExternalUrl,
            ImageHint = news.ImageHint
        }).ToList();

        return new PagedResult<NewsDto>
        {
            Items = forumResponseDTOs,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };
    }

}
