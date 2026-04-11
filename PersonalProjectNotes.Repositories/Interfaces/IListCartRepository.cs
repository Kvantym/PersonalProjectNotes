using PersonalProjectNotes.Domain.Entities;

namespace PersonalProjectNotes.Repositories.Interfaces
{
    public interface IListCartRepository
    {
        public Task Create(ListCart Listcart);
        public Task Update(ListCart Listcart);
        public Task Delete(ListCart Listcart);
        public Task<ListCart> GetListCart(Guid CartListID);
        public Task<List<ListCart>> GetListCarts(Guid UserID);
        public Task AddActivity(ActivityListCart activity);
        public Task<List<Cart>> GetListCartsByListId(Guid ListCartId);
       public Task<bool> ExistsAsync(Guid listCartId);
        //public Task<List<ListCart>> GetLiastCartsByBoardId(Guid boardId);
        public Task<List<ActivityListCart>> GetListCartActivityById(Guid listCartId);
        public Task<ListCart> GetListCartById(Guid listCartId);

        public Task UpdateCartListArchiveStatus(ListCart listCart, bool isArchive);
        public Task<List<ListCart>> GetListCartsByBoardIdAndIsArchive(Guid boardId, bool isArchive);
        public Task<List<ListCart>> SearchListCartByName(Guid boardId, string cartName, bool isArchive);
    }
}
