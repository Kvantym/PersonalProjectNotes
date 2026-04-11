

using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface ICommentServices
    {
        public Task AddCommentToCart(Guid cartId, Guid userId, string content);
        public Task<List<CommentResponse>> GetCommentsByCartId(Guid cartId, Guid userId);
        public Task DeleteComment(Guid commentId, Guid userId);
        public Task<Comment> GetOrThrowComment(Guid commentId);
    }
}
