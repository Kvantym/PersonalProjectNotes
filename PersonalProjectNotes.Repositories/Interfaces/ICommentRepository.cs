

using PersonalProjectNotes.Domain.Entities;

namespace PersonalProjectNotes.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        public Task AddComment(Guid cartId, Guid userId, string content);
        public Task<List<Comment>> GetCommentsByCartId(Guid cartId);
        public Task DeleteComment(Comment comment);
        public Task<Comment> GetCommentById(Guid commentId);
    }
}
