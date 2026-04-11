
namespace PersonalProjectNotes.Domain.Response
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid CartId { get; set; }
    }
}
