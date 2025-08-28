using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Entities
{
    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public UserAction Action { get; set; }
        public ICollection<ListCart> ListCart { get; set; } = new List<ListCart>();
        public ICollection<ActivityBoard> ActivityBoards { get; set; } = new List<ActivityBoard>();
    }
}
