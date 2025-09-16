using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Cart;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface ICartService
    {
        public Task CreateCart(CreateCartRequest createCartRequest, Guid userId, Guid cartListId);
        public Task UpdateCart(Guid cartId, UpdateCartRequest updateCartRequest, Guid userId);
        public Task DeleteCart(Guid cartId, Guid userId);
        public Task<CartResponse> GetCart(Guid cartId);
        public Task<List<CartResponse>> GetCarts(Guid userId);
       // public Task AddActivityToCart(Guid cartId, UserAction userAction, Guid userId, Cart? previousCartState = null, string? previousListName = null, string? targetListName = null);
        public Task MoveToCardList(Guid cartId, Guid cartLisID, Guid userId);
        public Task<Cart> GetOrThrowCart(Guid cartId);
        public Task<List<Cart>> GetOrThrowCartsByUserId(Guid userId);
        public  Task<List<Cart>> GetCartsByListCart(Guid ListCartId);
        public Task<List<ActivityCartResponse>> GetActivityCart(Guid cartId);
    }
}
