using PersonalProjectNotes.Domain.Request.Account;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<string> LoginAsync(LoginRequest request);

        public Task<string> RegisterUserAsync(RegisterUserRequest request);

        public Task<UserResponse> GetCurrentUserAsync();
    }
}
