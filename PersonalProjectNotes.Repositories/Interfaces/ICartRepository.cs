using PersonalProjectNotes.Domain.Entities;

namespace PersonalProjectNotes.Repositories.Interfaces
{
    public interface ICartRepository
    {
        public Task Create(Cart cart);
        public Task Update(Cart cart);
        public Task Delete(Cart cart);
        public Task<Cart> GetCart(Guid CartID);
        public Task<List<Cart>> GetCarts(Guid UserID);
        public Task AddActivity(ActivityCart activity);
        public  Task<List<Cart>> GetCartsByCartList(Guid cartListId);
        public Task<List<ActivityCart>> GetActivityCart(Guid cartId);

    }
}
