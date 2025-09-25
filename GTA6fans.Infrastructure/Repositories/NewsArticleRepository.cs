using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;

namespace GTA6fans.Infrastructure.Repositories;

public class NewsArticleRepository : GenericRepository<NewsArticle>, INewsArticleRepository
{
    public NewsArticleRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NewsArticle>> GetRecentArticlesAsync(int limit = 10)
    {
        return await _collection.Find(x => !x.IsDeleted)
            .SortByDescending(x => x.PublishedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<NewsArticle>> GetMostLikedAsync(int limit = 10)
    {
        return await _collection.Find(x => !x.IsDeleted)
            .SortByDescending(x => x.Likes)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<NewsArticle>> GetByAuthorAsync(string author)
    {
        return await _collection.Find(x => x.Author == author && !x.IsDeleted)
            .SortByDescending(x => x.PublishedAt)
            .ToListAsync();
    }

    public async Task<NewsArticle?> GetByExternalUrlAsync(string externalUrl)
    {
        return await _collection.Find(x => x.ExternalUrl == externalUrl && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IncrementLikesAsync(string articleId)
    {
        var filter = Builders<NewsArticle>.Filter.Eq(x => x.Id, articleId);
        var update = Builders<NewsArticle>.Update
            .Inc(x => x.Likes, 1)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DecrementLikesAsync(string articleId)
    {
        var filter = Builders<NewsArticle>.Filter.Eq(x => x.Id, articleId);
        var update = Builders<NewsArticle>.Update
            .Inc(x => x.Likes, -1)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<NewsArticle>> SearchByTitleAsync(string searchTerm)
    {
        var filter = Builders<NewsArticle>.Filter.And(
            Builders<NewsArticle>.Filter.Text(searchTerm),
            Builders<NewsArticle>.Filter.Eq(x => x.IsDeleted, false)
        );

        return await _collection.Find(filter)
            .SortByDescending(x => x.PublishedAt)
            .ToListAsync();
    }
}