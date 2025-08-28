namespace PersonalProjectNotes.Domain.Entities
{
    public class ActivityCart : Activity
    {
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }
    }
}
