namespace PersonalProjectNotes.Domain.Request.Board
{
    public class UpdateBoardRequest
    {
        public string Name { get; set; }
        private DateTime _updatedAt = DateTime.UtcNow;
        public DateTime UpdatedAt { get => _updatedAt;}
    }
}
