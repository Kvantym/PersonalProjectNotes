using PersonalProjectNotes.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalProjectNotes.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public Guid ListCartId { get; set; }
        public ListCart ListCart { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsArchived { get; set; }
        public PriorityNote PriorityNote { get; set; }
        public StatusNote StatusNote { get; set; }
        //[NotMapped]
        //public UserAction Action { get; set; }
        public List<ActivityCart> ActivityCart { get; set; } = new List<ActivityCart>();
    }
}
