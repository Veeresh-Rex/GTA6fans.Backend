using System.Collections.Generic;

namespace GTA6fans.Application.DTOs;

public class HomeOverviewDto
{
    public long UsersCount { get; set; }
    public List<NewsDto> LatestNews { get; set; } = new();
    public List<ForumResponseDTO> TrendingTopics { get; set; } = new();
    public long ForumCount { get; set; }
}
