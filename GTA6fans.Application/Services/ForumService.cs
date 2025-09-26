using System.Text.RegularExpressions;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Helpers;
using GTA6fans.Application.Interfaces;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Domain.Models;

namespace GTA6fans.Application.Services;

public class ForumService : IForumService
{
    private readonly IForumTopicRepository _forumTopicRepository;
    private readonly IUserRepository _userRepository;
    private readonly IForumReplyRepository _forumReplyRepository;

    public ForumService(IForumTopicRepository forumTopicRepository, IUserRepository userRepository, IForumReplyRepository forumReplyRepository)
    {
        _forumTopicRepository = forumTopicRepository;
        _userRepository = userRepository;
        _forumReplyRepository = forumReplyRepository;
    }

    public async Task<ForumResponseDTO> GetForumbySlug(string slug)
    {
        var forum = (await _forumTopicRepository.FindAsync(x => x.Slug == slug.ToLower())).FirstOrDefault();
        if (forum == null)
        {
            throw new ArgumentException("Forum not found");
        }

        var replies = await _forumReplyRepository.GetByTopicIdAsync(forum.Id);

        ForumResponseDTO forumResponseDTO = new ForumResponseDTO()
        {
            Id = forum.Id,
            Author = new Author
            {
                Name = forum.AuthorName, 
            },
            Category = forum.Category,
            Slug = forum.Slug,
            Content = forum.Content,
            Images = forum.Images,
            Replies = BuildReplyTree(replies),
            RepliesCount = forum.ReplyCount,
            Title = forum.Title,
            Timestamp = forum.CreatedAt.ToString(),
            LastPost = new LastPost
            {
                Author = forum.LastActivity?.AuthorName ?? "",
                Timestamp = forum.LastActivity?.Timestamp.ToString() ?? ""
            }
        };

        return forumResponseDTO;
    }

    public async Task<PagedResult<ForumResponseDTO>> GetForumList(int page, int pageSize, bool? sortByPopularity)
    {
        PagedResult<ForumTopic> pagedResult = new PagedResult<ForumTopic>();

        if (sortByPopularity is true)
        {
            pagedResult = await _forumTopicRepository.GetPagedAsync(page, pageSize, null, x => x.OrderBy(e => e.ReplyCount));
        }
        else
        {
            pagedResult = await _forumTopicRepository.GetPagedAsync(page, pageSize, null, x => x.OrderBy(e => e.CreatedAt));
        }

        var forumResponseDTOs = pagedResult.Items.Select(forum => new ForumResponseDTO()
        {
            Id = forum.Id,
            Author = new Author
            {
                Name = forum.AuthorName,
            },
            Category = forum.Category,
            Content = forum.Content,
            Images = forum.Images,
            RepliesCount = forum.ReplyCount,
            Title = forum.Title,
            Timestamp = forum.CreatedAt.ToString("O"),
            LastPost = new LastPost
            {
                Author = forum.LastActivity?.AuthorName ?? "",
                Timestamp = forum.LastActivity?.Timestamp.ToString("O") ?? ""
            },
            Slug = forum.Slug
        }).ToList();

        PagedResult<ForumResponseDTO> result = new PagedResult<ForumResponseDTO>()
        {
            Items = forumResponseDTOs,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };

        return result;
    }

    public async Task<CreateForumResponseDTO> CreateTopicAsync(CreateForumRequestDTO requestDTO, string authorId)
    {
        var user = await _userRepository.GetByIdAsync(authorId);
        if(user == null)
        {
            throw new ArgumentException("Author not found");
        }

        string baseSlug = SlugHelper.GenerateSlug(requestDTO.Title);
        var existingSlugs = await _forumTopicRepository.FindAsync(x => x.Slug.StartsWith(baseSlug), x => x.Slug);

        ForumTopic forumTopic = new ForumTopic()
        {
            AuthorId = authorId,
            Category = requestDTO.Category,
            Content = requestDTO.Content,
            Title = requestDTO.Title,
            Slug = requestDTO.Title.ToLower().Replace(" ", "-"),
            AuthorName = user.DisplayName,
            Images = new List<string>(),
            LastActivity = new LastActivity()
            {
                AuthorId = authorId,
                AuthorName = user.DisplayName,
                Timestamp = DateTime.UtcNow
            }
        };
        string slug = baseSlug;

        if (existingSlugs.Contains(baseSlug))
        {
            // Find highest suffix
            int maxSuffix = existingSlugs
                .Select(s =>
                {
                    var match = Regex.Match(s, $@"^{Regex.Escape(baseSlug)}-(\d+)$");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                })
                .Max();

            slug = $"{baseSlug}-{maxSuffix + 1}";
        }

        forumTopic.Slug = slug;

        await _forumTopicRepository.CreateAsync(forumTopic);

        return new() { Slug = $"{forumTopic.Slug}" };
    }

    public async Task AddReplyAsync(CreateReplyRequest request)
    {
        var topic = await _forumTopicRepository.GetByIdAsync(request.TopicId);
        var author = await _userRepository.GetByIdAsync(request.AuthorId);
        if (topic == null)
        {
            throw new ArgumentException("Topic not found");
        }

        if (author == null)
        {
            throw new ArgumentException("Author not found");
        }

        ForumReply forumReply = new ForumReply()
        {
            AuthorId = request.AuthorId,
            Content = request.Content,
            ParentId = request.ParentId,
            TopicId = request.TopicId,
            AuthorName = author.DisplayName
        };

        await _forumReplyRepository.CreateAsync(forumReply);
        await _forumTopicRepository.UpdateByReplyAsync(topic.Id, new LastActivity() { 
        AuthorId = request.AuthorId,
            AuthorName = author.DisplayName,
            Timestamp = DateTime.UtcNow
        });
    }

    private static List<Reply> BuildReplyTree(IEnumerable<ForumReply> allReplies)
    {
        if(allReplies.Count() == 0)
        {
            return new List<Reply>();
        }

        var replyLookup = allReplies
            .ToDictionary(r => r.Id.ToString(), r => new Reply
            {
                Id = r.Id,
                Author = new Author { Name = r.AuthorName },
                Timestamp = r.CreatedAt.ToString("o"),
                Content = r.Content,
                Replies = new List<Reply>()
            });

        var roots = new List<Reply>();

        foreach (var reply in allReplies)
        {
            if (!string.IsNullOrEmpty(reply.ParentId) &&
                replyLookup.ContainsKey(reply.ParentId))
            {
                // Attach child to parent
                replyLookup[reply.ParentId].Replies.Add(replyLookup[reply.Id.ToString()]);
            }
            else
            {
                // No parent → root node
                roots.Add(replyLookup[reply.Id.ToString()]);
            }
        }

        return roots;
    }

}
