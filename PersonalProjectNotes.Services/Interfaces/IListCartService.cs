using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.ListCart;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IListCartService
    {
        public Task CreateAsync(CreateListCartRequest listCartRequest, Guid userId, Guid boardId);
        public Task UpdateAsync(Guid cartListId, UpdateListCartRequest listCartRequest, Guid userId);
        public Task DeleteAsync(Guid listCartId, Guid userId);
        public Task<ListCartResponse> GetListCartAsync(Guid listCartId);
        public Task<List<ListCartResponse>> GetListCartsAsync(Guid userId);
      //  public Task AddActivityToCartList(Guid cartListId, UserAction userAction, Guid userId, ListCart? previousCartListState = null, string? previousBoardName = null, string? targetBoardName = null);
        public Task MoveToBoard(Guid listCartId, Guid boardId, Guid userId);
        public Task<ListCart> GetOrThrowListCart(Guid listCartId);
        public Task<List<Cart>> GetOrThrowGartsByList(Guid listCartId);
        //  public Task<List<ListCart>> GetLiastCartByBoardId(Guid boardId);
        public Task<List<ListCart>> GetListCartByBoardId(Guid boardId, bool isArchive);
        public Task<List<ActivityListCartResponse>> GetListCartActivityByListId(Guid cartListId);
        public Task<ListCart> GetListCartById(Guid cartListId);
        public Task<bool> ListCartExists(Guid cartLisId);
        public Task UpdateCartListArchiveStatus(Guid userId,Guid listCartId, bool isArchive);
        public Task<List<ListCart>> SearchListCartByName(Guid boardId, string cartName, bool isArchive);

    }
}
