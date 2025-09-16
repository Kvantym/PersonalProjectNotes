using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Request.Cart
{
    public class CreateCartRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public PriorityNote PriorityNote { get; set; }
        public StatusNote StatusNote { get; set; }
        public UserAction Action = UserAction.Create;

    }
}
