using System.Linq.Expressions;
using System.Text.RegularExpressions;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Helpers;
using GTA6fans.Application.Interfaces;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Domain.Models;

namespace GTA6fans.Application.Services;

public class ForumService : IForumService
{
    private readonly IForumTopicRepository _forumTopicRepository;
    private readonly IUserRepository _userRepository;
    private readonly IForumReplyRepository _forumReplyRepository;
    private readonly IReactionRepository _reactionRepository;

    public ForumService(IForumTopicRepository forumTopicRepository, IUserRepository userRepository, IForumReplyRepository forumReplyRepository, IReactionRepository reactionRepository)
    {
        _forumTopicRepository = forumTopicRepository;
        _userRepository = userRepository;
        _forumReplyRepository = forumReplyRepository;
        _reactionRepository = reactionRepository;
    }

    public async Task<ForumResponseDTO> GetForumbySlug(string slug, string? userId)
    {
        var forum = (await _forumTopicRepository.FindAsync(x => x.Slug == slug.ToLower())).FirstOrDefault();
        if (forum == null)
        {
            throw new ArgumentException("Forum not found");
        }

        var replies = await _forumReplyRepository.GetByTopicIdAsync(forum.Id);
        var documentIds = replies.Select(r => r.Id).ToList(); documentIds.Add(forum.Id);

        var reactions = await _reactionRepository.FindAsync(r => documentIds.Contains(r.DocumentId));

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
            Replies = BuildReplyTree(replies, reactions, userId),
            RepliesCount = forum.ReplyCount,
            Title = forum.Title,
            Timestamp = forum.CreatedAt.ToString(),
            LastPost = new LastPost
            {
                Author = forum.LastActivity?.AuthorName ?? "",
                Timestamp = forum.LastActivity?.Timestamp.ToString() ?? ""
            },
            Reactions = reactions.Where(x => x.DocumentId == forum.Id && x.DocumentType == DocumentType.Topic).GroupBy(e => e.Emoji).ToDictionary(g => g.Key, g => g.Count()),
            MyReaction = userId == null ? null : reactions.FirstOrDefault(x => x.DocumentId == forum.Id && x.DocumentType == DocumentType.Topic && x.UserId == userId)?.Emoji
        };

        return forumResponseDTO;
    }

    public async Task<PagedResult<ForumResponseDTO>> GetForumList(string? userId, string? query, string? scope, string? category, int page, int pageSize, bool? sortByPopularity)
    {
        // Build filter condition
        Expression<Func<ForumTopic, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowered = query.ToLower();
            filter = x => x.Title.ToLower().Contains(lowered);
        }

        if (!string.IsNullOrWhiteSpace(scope) && scope.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                filter = filter.AndAlso(x => x.AuthorId == userId);
            }
        }


        // 🏷️ Category filter
        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filter = filter.AndAlso(x => x.Category.ToLower() == category.ToLower());
        }


        // Choose sorting
        Func<IQueryable<ForumTopic>, IOrderedQueryable<ForumTopic>> orderBy;
        if (sortByPopularity is true)
        {
            orderBy = x => x.OrderByDescending(e => e.ReplyCount);
        }
        else
        {
            orderBy = x => x.OrderByDescending(e => e.CreatedAt);
        }

        // Get data
        var pagedResult = await _forumTopicRepository.GetPagedAsync(page, pageSize, filter, orderBy);

        // Map to DTO
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

        return new PagedResult<ForumResponseDTO>
        {
            Items = forumResponseDTOs,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };
    }


    public async Task<CreateForumResponseDTO> CreateTopicAsync(CreateForumRequestDTO requestDTO, string authorId)
    {
        var user = await _userRepository.GetByIdAsync(authorId);
        if (user == null)
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
            Images = requestDTO.Images,
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

    public async Task<Reply> AddReplyAsync(CreateReplyRequest request)
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
        await _forumTopicRepository.UpdateByReplyAsync(topic.Id, new LastActivity()
        {
            AuthorId = request.AuthorId,
            AuthorName = author.DisplayName,
            Timestamp = DateTime.UtcNow
        });

        return new Reply()
        {
            Id = forumReply.Id,
            Author = new Author { Name = author.DisplayName },
            Timestamp = forumReply.CreatedAt.ToString("o"),
            Content = forumReply.Content
        };
    }

    public async Task SubmitReactionAsync(ReactionRequestDto request, string userId)
    {

        if (request.DocumentType != DocumentType.Topic && request.DocumentType != DocumentType.Comment)
        {
            throw new ArgumentException("Invalid reaction type");
        }

        if (request.IsRevoking)
        {
            var reactionToDelete = await _reactionRepository.FindFirstOrDefaultAsync(r => r.UserId == userId && r.DocumentId == request.DocumentId);
            if (reactionToDelete != null)
            {
                await _reactionRepository.DeleteAsync(reactionToDelete.Id);
            }
            return;
        }

        var existingReaction = await _reactionRepository.FindFirstOrDefaultAsync(r => r.UserId == userId && r.DocumentId == request.DocumentId);
        if (existingReaction != null)
        {
            existingReaction.Emoji = request.Emoji;

            await _reactionRepository.UpdateAsync(existingReaction);
        }
        else
        {
            // Add new reaction
            await _reactionRepository.CreateAsync(new Domain.Entities.Reaction
            {
                UserId = userId,
                Emoji = request.Emoji,
                DocumentId = request.DocumentId,
                DocumentType = request.DocumentType,

            });
        }
    }

    private static List<Reply> BuildReplyTree(IEnumerable<ForumReply> allReplies, IEnumerable<Domain.Entities.Reaction> reactions, string? userId)
    {
        if (allReplies.Count() == 0)
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
                Replies = new List<Reply>(),
                Reactions = reactions.Where(x => x.DocumentId == r.Id && x.DocumentType == DocumentType.Comment).GroupBy(x => x.Emoji).ToDictionary(g => g.Key, g => g.Count()),
                MyReaction = userId == null ? null : reactions.FirstOrDefault(x => x.DocumentId == r.Id && x.DocumentType == DocumentType.Comment && x.UserId == userId)?.Emoji
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
