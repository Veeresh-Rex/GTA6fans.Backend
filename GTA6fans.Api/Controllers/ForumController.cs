using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GTA6fans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForumController : ControllerBase
{
    private readonly IForumService _forumService;
    private readonly ILogger<ForumController> _logger;

    public ForumController(IForumService forumService, ILogger<ForumController> logger)
    {
        _forumService = forumService;
        _logger = logger;
    }


    [HttpGet("{slug}")]
    public async Task<ActionResult<ForumResponseDTO>> GetForumBySlug(string slug)
    {
        var forum = await _forumService.GetForumbySlug(slug);

        return Ok(forum);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ForumTopic>>> GetForums([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var pagedData = await _forumService.GetForumList(page, pageSize, null);
        return Ok(pagedData);
    }

    [Authorize]
    [HttpPost("reply")]
    public async Task<ActionResult> PostReply(CreateReplyRequest createReplyRequest)
    {
        createReplyRequest.AuthorId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _forumService.AddReplyAsync(createReplyRequest);

        return Ok(true);
    }

    [Authorize(Roles = "1")]
    [HttpPost("new")]
    public async Task<ActionResult<CreateForumResponseDTO>> CreateForum([FromBody]CreateForumRequestDTO request)
    {
        try
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var newForum = await _forumService.CreateTopicAsync(request, userId);
            return Ok(newForum);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

}