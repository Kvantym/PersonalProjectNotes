using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Domain.Response
{
    public class ActivityListCartResponse
    {
      public UserAction  Action { get; set; }
        public string  ActivityInformation { get; set; }
        public DateTime  ActivityTime { get; set; }
    }
}
