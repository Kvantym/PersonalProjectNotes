using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IActivityService
    {
        Task AddActivityToCart(Guid cartId, UserAction userAction, Guid userId, Cart? previousCartState = null, string? previousListName = null, string? targetListName = null);
        Task AddActivityToCartList(Guid cartListId, UserAction userAction, Guid userId, ListCart? previousCartListState = null, string? previousBoardName = null, string? targetBoardName = null, Cart? deletedCart = null);
        public Task AddActivityToBoard(Guid boardId, UserAction userAction, Guid userId, Board? previousBoardState = null, ListCart? deleteListCart = null, ListCart? CreatelistCart = null, ApplicationUser? coloboration = null);

    }
}
