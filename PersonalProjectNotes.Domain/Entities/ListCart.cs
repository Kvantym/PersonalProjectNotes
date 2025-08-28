namespace PersonalProjectNotes.Domain.Entities
{
    public class ListCart
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid BoardId { get; set; }
        public Board Board { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<ActivityListCart> ActivityListCarts { get; set; } = new List<ActivityListCart>();

        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}
