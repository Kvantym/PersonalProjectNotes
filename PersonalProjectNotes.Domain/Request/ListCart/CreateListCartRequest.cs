namespace PersonalProjectNotes.Domain.Request.ListCart
{
    public class CreateListCartRequest
    {
        public string Name { get; set; }
        private DateTime _createdAt = DateTime.UtcNow;
        public DateTime CreatedAt { get => _createdAt; }

    }
}
