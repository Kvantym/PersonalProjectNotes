using Microsoft.EntityFrameworkCore;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Repositories.Interfaces;

namespace PersonalProjectNotes.Repositories.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddComment(Guid cartId, Guid userId, string content)
        {
            await _context.Comments.AddAsync(new Domain.Entities.Comment
            {
                Id = Guid.NewGuid(),
                CartId = cartId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task DeleteComment(Comment comment)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Comment> GetCommentById(Guid commentId)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<List<Comment>> GetCommentsByCartId(Guid cartId)
        {
            return await _context.Comments.Where(c => c.CartId == cartId).ToListAsync();
        }


    }
}
