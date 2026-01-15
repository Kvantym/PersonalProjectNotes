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
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        [NotMapped]
        public UserAction Action { get; set; }
        public ICollection<ListCart> ListCart { get; set; } = new List<ListCart>();
        public ICollection<ActivityBoard> ActivityBoards { get; set; } = new List<ActivityBoard>();
    }
}
