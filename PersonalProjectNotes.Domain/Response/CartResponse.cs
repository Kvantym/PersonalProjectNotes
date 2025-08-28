using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Response
{
    public class CartResponse
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public Guid UserId { get; set; }
        public PriorityNote PriorityNote { get; set; }
        public StatusNote StatusNote { get; set; }
        public UserAction Action { get; set; }
        public Guid ListCartId { get; set; }
        public List<ActivityCartResponse> ActivityCart { get; set; } = new List<ActivityCartResponse>();
    }
}
