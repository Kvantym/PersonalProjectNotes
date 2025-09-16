using PersonalProjectNotes.Domain.Entities;

namespace PersonalProjectNotes.Domain.Response
{
    public class BoardResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public ICollection<ListCartResponse> ListCart { get; set; } = new List<ListCartResponse>();
        public ICollection<ActivityBoardResponse> ActivityBoards { get; set; } = new List<ActivityBoardResponse>();
    }
}
