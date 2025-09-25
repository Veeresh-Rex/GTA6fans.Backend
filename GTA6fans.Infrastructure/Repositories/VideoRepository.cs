using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;

namespace GTA6fans.Infrastructure.Repositories;

public class VideoRepository : GenericRepository<Video>, IVideoRepository
{
    public VideoRepository(MongoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Video>> GetByStatusAsync(VideoStatus status)
    {
        return await _collection.Find(x => x.Status == status && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Video>> GetBySubmittedByAsync(string submittedBy)
    {
        return await _collection.Find(x => x.SubmittedBy == submittedBy && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Video>> GetByAuthorAsync(string author)
    {
        return await _collection.Find(x => x.Author == author && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Video?> GetByYoutubeIdAsync(string youtubeId)
    {
        return await _collection.Find(x => x.YoutubeId == youtubeId && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsYoutubeIdExistsAsync(string youtubeId)
    {
        var video = await _collection.Find(x => x.YoutubeId == youtubeId && !x.IsDeleted)
            .FirstOrDefaultAsync();
        return video != null;
    }

    public async Task<bool> UpdateStatusAsync(string videoId, VideoStatus status)
    {
        var filter = Builders<Video>.Filter.Eq(x => x.Id, videoId);
        var update = Builders<Video>.Update
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<Video>> GetApprovedVideosAsync(int limit = 20)
    {
        return await _collection.Find(x => x.Status == VideoStatus.Approved && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Video>> GetPendingVideosAsync()
    {
        return await _collection.Find(x => x.Status == VideoStatus.Pending && !x.IsDeleted)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }
}