using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<ApplicationUser> GetByNameAsync(string userName);
        public Task UpdateAsync(ApplicationUser user);
        public Task<List<UserResponse>> GetAllAsync();
        public Task DeleteAsync(Guid userId);
        public Task<ApplicationUser> GetByIdAsync(Guid userId);
        public Task<List<ApplicationUser>> GetUsersByIdsAsync(List<Guid> userIds);
    }
}
