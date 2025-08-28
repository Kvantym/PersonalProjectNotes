
using PersonalProjectNotes.Services.Exceptions;
using System.Security.Claims;

namespace PersonalProjectNotes.Services
{
    public static class ClaimPrincipeEastensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
      ?? throw new AuthorizationException("Користувача не знайдено");

            if (!Guid.TryParse(userIdString, out var userId))
                throw new AuthorizationException("Неправильний формат Id користувача");

            return userId;
        }
    }
    
}
