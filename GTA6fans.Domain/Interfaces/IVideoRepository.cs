using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Domain.Interfaces;

public interface IVideoRepository : IGenericRepository<Video>
{
    Task<IEnumerable<Video>> GetByStatusAsync(VideoStatus status);
    Task<IEnumerable<Video>> GetBySubmittedByAsync(string submittedBy);
    Task<IEnumerable<Video>> GetByAuthorAsync(string author);
    Task<Video?> GetByYoutubeIdAsync(string youtubeId);
    Task<bool> IsYoutubeIdExistsAsync(string youtubeId);
    Task<bool> UpdateStatusAsync(string videoId, VideoStatus status);
    Task<IEnumerable<Video>> GetApprovedVideosAsync(int limit = 20);
    Task<IEnumerable<Video>> GetPendingVideosAsync();
}