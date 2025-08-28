using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.ListCart;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IListCartService
    {
        public Task CreateAsync(CreateListCartRequest listCart, Guid userID, Guid boardId);
        public Task UpdateAsync(Guid cartListId, UpdateListCartRequest listCart, Guid userID);
        public Task DeleteAsync(Guid listCartId, Guid userID);
        public Task<ListCartResponse> GetListCartAsync(Guid listCartId);
        public Task<List<ListCartResponse>> GetListCartsAsync(Guid userId);
        public Task AddActivityToCartList(Guid CartListID, UserAction Action, Guid UserID);
        public Task MoveToBoard(Guid listCartId, Guid boardId, Guid userID);

    }
}
