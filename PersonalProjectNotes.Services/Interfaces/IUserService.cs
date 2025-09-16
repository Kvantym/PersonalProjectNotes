using PersonalProjectNotes.Domain.Entities;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IUserService
    {
        public Task<ApplicationUser> GetOrThrowUser(Guid UserID);
        public Task UpdateUserName(Guid userId, string newUserName);
        public Task UpdateUserPassword(Guid userId, string newUserPassword);
        public Task UpdateUserEmail(Guid userId, string newUserEmail);
    }

}
