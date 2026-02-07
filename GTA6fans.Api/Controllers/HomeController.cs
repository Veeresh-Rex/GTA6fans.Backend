using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GTA6fans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly INewsService _newsService;
    private readonly IForumService _forumService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IUserService userService, 
        INewsService newsService, 
        IForumService forumService,
        ILogger<HomeController> logger)
    {
        _userService = userService;
        _newsService = newsService;
        _forumService = forumService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<HomeOverviewDto>> GetOverview()
    {
        try
        {
            var userCountTask = _userService.GetUsersCountAsync();
            var latestNewsTask = _newsService.GetNewsList(null, null, 1, 5);
            var trendingTopicsTask = _forumService.GetForumList(null, null, null, null, 1, 5, true);

            await Task.WhenAll(userCountTask, latestNewsTask, trendingTopicsTask);

            var overview = new HomeOverviewDto
            {
                UsersCount = await userCountTask,
                LatestNews = (await latestNewsTask).Items,
                TrendingTopics = (await trendingTopicsTask).Items,
                ForumCount = (await trendingTopicsTask).TotalCount
            };

            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching home overview");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
