using PersonalProjectNotes.Domain.Entities;
using System.Text.Json.Serialization;

namespace PersonalProjectNotes.Domain.Request.Account
{
    public class RegisterUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
