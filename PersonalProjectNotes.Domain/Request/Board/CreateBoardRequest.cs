using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Request.Board
{
    public class CreateBoardRequest
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get => _createdAt; }

        private DateTime _createdAt = DateTime.UtcNow;
    }
}
