using GTA6fans.Domain.Entities;

namespace GTA6fans.Domain.Interfaces;

public interface INewsArticleRepository : IGenericRepository<NewsArticle>
{
    Task<IEnumerable<NewsArticle>> GetRecentArticlesAsync(int limit = 10);
    Task<IEnumerable<NewsArticle>> GetMostLikedAsync(int limit = 10);
    Task<IEnumerable<NewsArticle>> GetByAuthorAsync(string author);
    Task<NewsArticle?> GetByExternalUrlAsync(string externalUrl);
    Task<bool> IncrementLikesAsync(string articleId);
    Task<bool> DecrementLikesAsync(string articleId);
    Task<IEnumerable<NewsArticle>> SearchByTitleAsync(string searchTerm);
}