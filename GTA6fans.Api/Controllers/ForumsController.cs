using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GTA6fans.Api.Attributes;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GTA6fans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForumsController : ControllerBase
{
    private readonly IForumService _forumService;
    private readonly ILogger<ForumsController> _logger;

    public ForumsController(IForumService forumService, ILogger<ForumsController> logger)
    {
        _forumService = forumService;
        _logger = logger;
    }


    [HttpGet("{slug}")]
    [OptionalAuthorize]
    public async Task<ActionResult<ForumResponseDTO>> GetForumBySlug(string slug)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var forum = await _forumService.GetForumbySlug(slug, userId);

        return Ok(forum);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ForumResponseDTO>>> GetForums([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sort = "popular")
    {
        var pagedData = await _forumService.GetForumList(page, pageSize, sort != "newest");
        return Ok(pagedData);
    }

    [Authorize]
    [HttpPost("reply")]
    public async Task<ActionResult> PostReply(CreateReplyRequest createReplyRequest)
    {
        createReplyRequest.AuthorId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reply = await _forumService.AddReplyAsync(createReplyRequest);

        return Ok(reply);
    }

    [Authorize]
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

    [Authorize]
    [HttpPost("reaction")]
    public async Task<ActionResult<CreateForumResponseDTO>> SubmitReaction([FromBody] ReactionRequestDto request)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("User ID not found in token");
        }

        await _forumService.SubmitReactionAsync(request, userId);

        return Ok();
    }
}