using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Request.Board
{
    public class CreateBoardRequest
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get => _createdAt; }
        public UserAction UserAction { get => _userAction; }

        private DateTime _createdAt = DateTime.UtcNow;
        private UserAction _userAction = UserAction.Create;
    }
}
