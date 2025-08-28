using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Entities
{
    public class Activity
    {
        public Guid Id { get; set; }
        public UserAction Action { get; set; }
        public string ActivityInformation { get; set; }
        public Guid UserId { get; set; }
        public DateTime ActivityTime { get; set; }
    }
}
