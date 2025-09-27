using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Application.Services;
using GTA6fans.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GTA6fans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public NewsController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ForumResponseDTO>>> GetForums([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sort = "popular")
    {
       // var pagedData = await _forumService.GetForumList(page, pageSize, sort != "newest");
        return Ok();
    }


}