using AutoMapper;
using Microsoft.Extensions.Logging;
using CommunityService.DTOs;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            ILikeRepository likeRepository,
            IMapper mapper,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommentResponse?> GetCommentByIdAsync(long id, long? currentUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return null;

            var response = _mapper.Map<CommentResponse>(comment);

            if (currentUserId.HasValue)
            {
                response.IsLiked = await _likeRepository.ExistsByUserAndTargetAsync(currentUserId.Value, LikeTargetType.Comment, id);
            }

            // TODO: 从用户服务获取作者信息
            response.AuthorName = $"User{comment.AuthorId}";
            response.AuthorAvatar = null;

            // 加载回复
            var replies = await _commentRepository.GetRepliesAsync(id, 1, 10);
            response.Replies = _mapper.Map<List<CommentResponse>>(replies);

            return response;
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsAsync(CommentQueryParams queryParams, long? currentUserId)
        {
            var comments = await _commentRepository.GetAllAsync(
                queryParams.Page, queryParams.PageSize, queryParams.PostId,
                queryParams.AuthorId, queryParams.ParentId, queryParams.SortBy);

            var responses = new List<CommentResponse>();
            foreach (var comment in comments)
            {
                var response = _mapper.Map<CommentResponse>(comment);
                // TODO: 从用户服务获取作者信息
                response.AuthorName = $"User{comment.AuthorId}";
                response.AuthorAvatar = null;

                if (currentUserId.HasValue)
                {
                    response.IsLiked = await _likeRepository.ExistsByUserAndTargetAsync(currentUserId.Value, LikeTargetType.Comment, comment.Id);
                }

                responses.Add(response);
            }

            return responses;
        }

        public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, long authorId)
        {
            // 验证帖子是否存在
            var postExists = await _postRepository.ExistsAsync(request.PostId);
            if (!postExists)
            {
                throw new ArgumentException("Post not found");
            }

            // 如果存在父评论，验证父评论是否存在且属于同一帖子
            if (request.ParentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value);
                if (parentComment == null || parentComment.PostId != request.PostId)
                {
                    throw new ArgumentException("Parent comment not found or does not belong to the same post");
                }
            }

            var comment = _mapper.Map<Comment>(request);
            comment.AuthorId = authorId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            var createdComment = await _commentRepository.CreateAsync(comment);
            await _postRepository.IncrementCommentCountAsync(request.PostId, 1);

            return _mapper.Map<CommentResponse>(createdComment);
        }

        public async Task<CommentResponse?> UpdateCommentAsync(long id, UpdateCommentRequest request, long currentUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return null;

            if (comment.AuthorId != currentUserId)
            {
                // TODO: 检查是否是管理员
                throw new UnauthorizedAccessException("You are not authorized to update this comment");
            }

            _mapper.Map(request, comment);
            comment.UpdatedAt = DateTime.UtcNow;

            var updatedComment = await _commentRepository.UpdateAsync(comment);
            return _mapper.Map<CommentResponse>(updatedComment);
        }

        public async Task<bool> DeleteCommentAsync(long id, long currentUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return false;

            if (comment.AuthorId != currentUserId)
            {
                // TODO: 检查是否是管理员
                throw new UnauthorizedAccessException("You are not authorized to delete this comment");
            }

            var result = await _commentRepository.DeleteAsync(id);
            if (result)
            {
                await _postRepository.IncrementCommentCountAsync(comment.PostId, -1);
            }
            return result;
        }

        public async Task<bool> SoftDeleteCommentAsync(long id, long currentUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return false;

            if (comment.AuthorId != currentUserId)
            {
                // TODO: 检查是否是管理员
                throw new UnauthorizedAccessException("You are not authorized to delete this comment");
            }

            return await _commentRepository.SoftDeleteAsync(id);
        }

        public async Task<bool> LikeCommentAsync(long commentId, long userId)
        {
            var alreadyLiked = await _likeRepository.ExistsByUserAndTargetAsync(userId, LikeTargetType.Comment, commentId);
            if (alreadyLiked) return false;

            var like = new Like
            {
                UserId = userId,
                TargetType = LikeTargetType.Comment,
                TargetId = commentId,
                CreatedAt = DateTime.UtcNow
            };

            await _likeRepository.CreateAsync(like);
            await _commentRepository.IncrementLikeCountAsync(commentId, 1);
            return true;
        }

        public async Task<bool> UnlikeCommentAsync(long commentId, long userId)
        {
            var like = await _likeRepository.GetByUserAndTargetAsync(userId, LikeTargetType.Comment, commentId);
            if (like == null) return false;

            await _likeRepository.DeleteAsync(like.Id);
            await _commentRepository.IncrementLikeCountAsync(commentId, -1);
            return true;
        }

        public async Task<IEnumerable<CommentResponse>> GetRepliesAsync(long parentId, int page, int pageSize, long? currentUserId)
        {
            var replies = await _commentRepository.GetRepliesAsync(parentId, page, pageSize);
            var responses = new List<CommentResponse>();

            foreach (var reply in replies)
            {
                var response = _mapper.Map<CommentResponse>(reply);
                // TODO: 从用户服务获取作者信息
                response.AuthorName = $"User{reply.AuthorId}";
                response.AuthorAvatar = null;

                if (currentUserId.HasValue)
                {
                    response.IsLiked = await _likeRepository.ExistsByUserAndTargetAsync(currentUserId.Value, LikeTargetType.Comment, reply.Id);
                }

                responses.Add(response);
            }

            return responses;
        }

        public async Task<bool> UpdateCommentStatusAsync(long id, CommentStatus status, long currentUserId)
        {
            // TODO: 检查是否是管理员
            return await _commentRepository.UpdateStatusAsync(id, status);
        }

        public async Task<CommentStatsResponse> GetCommentStatsAsync(long id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return null;

            var replyCount = await _commentRepository.GetCountAsync(null, null, id);

            return new CommentStatsResponse
            {
                CommentId = comment.Id,
                LikeCount = comment.LikeCount,
                ReplyCount = replyCount,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        public async Task<bool> IsLikedAsync(long commentId, long userId)
        {
            return await _likeRepository.ExistsByUserAndTargetAsync(userId, LikeTargetType.Comment, commentId);
        }
    }
}