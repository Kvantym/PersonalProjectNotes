namespace PersonalProjectNotes.Domain.Entities
{
    public class ActivityBoard : Activity
    {
        public Guid BoardId { get; set; }
        public Board Board { get; set; }

    }
}
