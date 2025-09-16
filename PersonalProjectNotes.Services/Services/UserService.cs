using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApplicationUser> GetOrThrowUser(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"User with ID {userId} not found");
            }
            return user;
        }
        public async Task UpdateUserName(Guid userId, string newUserName)
        {
            var user = await GetOrThrowUser(userId);
            await _userRepository.UpdateUserName(userId, newUserName);
        }
        public async Task UpdateUserPassword(Guid userId, string newUserPassword)
        {
            var user = await GetOrThrowUser(userId);
            await _userRepository.UpdatePassword(userId, newUserPassword);
        }
        public async Task UpdateUserEmail(Guid userId, string newUserEmail)
        {
            var user = await GetOrThrowUser(userId);
            await _userRepository.UpdateUserEmail(userId, newUserEmail);
        }
    }
}
