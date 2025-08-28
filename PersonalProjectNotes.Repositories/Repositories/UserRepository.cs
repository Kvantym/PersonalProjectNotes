using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;

namespace PersonalProjectNotes.Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task DeleteAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserResponse>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser> GetByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user;
        }

        public Task<ApplicationUser> GetByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationUser>> GetUsersByIdsAsync(List<Guid> userIds)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}
