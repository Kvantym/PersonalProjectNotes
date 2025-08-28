
namespace PersonalProjectNotes.Domain.Entities
{
    public class ActivityListCart : Activity
    {
        public Guid ListCartId { get; set; }
        public ListCart ListCart { get; set; }
    }
}
