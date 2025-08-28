using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Cart;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface ICartService
    {
        public Task CreateCart(CreateCartRequest createCartRequest, Guid UserID);
        public Task UpdateCart(Guid CartID, UpdateCartRequest updateCartRequest, Guid UserID);
        public Task DeleteCart(Guid CartID, Guid userID);
        public Task<CartResponse> GetCart(Guid CartID);
        public Task<List<CartResponse>> GetCarts(Guid UserID);
        public Task AddActivityToCart(Guid CartID, UserAction Action, Guid UserID);
        public Task MoveToCardList(Guid CartID, Guid cartLisID, Guid UserID);
    }
}
