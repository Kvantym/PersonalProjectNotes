using Microsoft.EntityFrameworkCore;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task UpdateAsync(ApplicationUser user);
        public Task<List<UserResponse>> GetAllAsync();
        public Task DeleteAsync(Guid userId);
        public Task<ApplicationUser> GetByIdAsync(Guid userId);
        public Task<List<ApplicationUser>> GetUsersByIdsAsync(List<Guid> userIds);
        public Task UpdateUserName(Guid userId, string newUserName);
        public Task UpdatePassword(Guid userId, string newPassword);
        public Task UpdateUserEmail(Guid userId, string newUserEmail);
        public Task<ApplicationUser> GetUserByName(string userName);
        public Task<ApplicationUser> GetUserByEmail(string userEmail);

    }
}
