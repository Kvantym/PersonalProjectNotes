
using Microsoft.AspNetCore.Identity;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;

namespace PersonalProjectNotes.Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public UserRepository(AppDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;

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
        public async Task UpdateUserName(Guid userId, string newUserName)
        {
            var user = await _context.Users.FindAsync(userId);

            user.UserName = newUserName;
            user.NormalizedUserName = newUserName.ToUpper();

            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePassword(Guid userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUserEmail(Guid userId, string newUserEmail)
        {
            var user = await _context.Users.FindAsync(userId);

            user.Email = newUserEmail;
            user.NormalizedEmail = newUserEmail.ToUpper();

            _context.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
