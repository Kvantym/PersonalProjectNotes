

using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class CommentServices : ICommentServices
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICartService _cartService;
        private readonly IUserService _userService;

        public CommentServices(ICommentRepository commentRepository, ICartService cartService, IUserService userService)
        {
            _commentRepository = commentRepository;
            _cartService = cartService;
            _userService = userService;
        }

        public async Task AddCommentToCart(Guid cartId, Guid userId, string content)
        {
            await _userService.GetOrThrowUser(userId);
            await _cartService.GetOrThrowCart(cartId);

            await _commentRepository.AddComment(cartId, userId, content);
        }

        public async Task<List<CommentResponse>> GetCommentsByCartId(Guid cartId, Guid userId)
        {
            await _cartService.GetOrThrowCart(cartId);

            var comments = await _commentRepository.GetCommentsByCartId(cartId);
            var userIds = comments.Select(c => c.UserId).Distinct();

            List<ApplicationUser?> users = new List<ApplicationUser?>();
            foreach (var id in userIds)
            {
                var user = await _userService.GetOrThrowUser(id);
                users.Add(user);
            }

            var userDict = users.ToDictionary(u => u.Id, u => u.UserName);

            return comments.Select(c => new CommentResponse
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserName = userDict.TryGetValue(c.UserId, out var name) ? name : "Unknown User",
                CartId = c.CartId
            }).ToList();
        }

        public async Task DeleteComment(Guid commentId, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);
            var comment = await GetOrThrowComment(commentId);

            if (comment.UserId != userId)
            {
                throw new BadRequestException("You can only delete your own comments");
            }
            else if (comment.UserId == userId)
            {
                await _commentRepository.DeleteComment(comment);
            }

        }

        public async Task<Comment> GetOrThrowComment(Guid commentId)
        {
            var comment = await _commentRepository.GetCommentById(commentId);
            if (comment == null)
            {
                throw new BadRequestException("Comment not found");
            }
            return comment;
        }
    }
}
