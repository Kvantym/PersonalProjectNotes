namespace PersonalProjectNotes.Domain.Response
{
    public class ListCartResponse
    {
        public Guid ListCartId { get; set; }
        public string Name { get; set; }
        public Guid BoardId { get; set; }

        public Guid UserId { get; set; }
        public ICollection<ActivityListCartResponse> ActivityListCarts { get; set; } = new List<ActivityListCartResponse>();

        public ICollection<CartResponse> Carts { get; set; } = new List<CartResponse>();
    }
}
