using PersonalProjectNotes.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalProjectNotes.Domain.Entities
{
    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Guid> Collaborators { get; set; } = new List<Guid>();
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsArchived { get; set; }
        public ICollection<ListCart> ListCart { get; set; } = new List<ListCart>();
        public ICollection<ActivityBoard> ActivityBoards { get; set; } = new List<ActivityBoard>();
    }
}
