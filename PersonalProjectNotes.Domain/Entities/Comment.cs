

namespace PersonalProjectNotes.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CartId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
    