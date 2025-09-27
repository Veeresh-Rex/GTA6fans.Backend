using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GTA6fans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly ILogger<UsersController> _logger;

    public NewsController(INewsService newsService, ILogger<UsersController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ForumResponseDTO>>> GetNews([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var pagedData = await _newsService.GetNewsList(null, query, page, pageSize);
        return Ok(pagedData);
    }

    [HttpPost]
    public async Task<ActionResult> CreateNewsArticle([FromBody] CreateNewsArticleRequest request)
    {
        try
        {
            var result = await _newsService.CreateNewsArticleAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating news article");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }


}