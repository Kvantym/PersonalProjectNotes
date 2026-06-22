using Microsoft.EntityFrameworkCore;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Repositories.Interfaces;
using System;

namespace PersonalProjectNotes.Repositories.Repositories
{
    public class ListCartRepository : IListCartRepository
    {
        private readonly AppDbContext _context;
        public ListCartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddActivity(ActivityListCart activity)
        {
            await _context.ActivityListCarts.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task Create(ListCart Listcart)
        {
            
           _context.ListCarts.AddAsync(Listcart);
           await _context.SaveChangesAsync();
        }

        public async Task Delete(ListCart Listcart)
        {
            _context.ListCarts.Remove(Listcart);
            await _context.SaveChangesAsync();
        }

        public async Task<ListCart> GetListCart(Guid listCartId)
        {
            return await _context.ListCarts
                .Include(l => l.Carts)
                    .ThenInclude(c => c.ActivityCart)
                .Include(l => l.ActivityListCarts)
                .FirstOrDefaultAsync(l => l.Id == listCartId);
        }


        public async Task<List<ListCart>> GetListCarts(Guid UserID)
        {
            return await _context.ListCarts
                .Include(carts => carts.Carts).ThenInclude(c => c.ActivityCart)
                .Include(ac => ac.ActivityListCarts)
                .Where(lc => lc.UserId == UserID)
                .ToListAsync();
        }

        public async Task Update(ListCart Listcart)
        {
            _context.ListCarts.Update(Listcart);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Cart>> GetListCartsByListId( Guid ListCartId)
        {
            return await _context.Carts.Include(ac=>ac.ActivityCart)
                   .Where(c => c.ListCartId == ListCartId)
                   .ToListAsync();
        }
        public async Task<bool> ExistsAsync(Guid listCartId)
        {
            return await _context.ListCarts.AnyAsync(lc => lc.Id == listCartId);
        }

        public async Task<List<ActivityListCart>> GetListCartActivityById(Guid listCartId)
        {
            return await _context.ActivityListCarts.Where(ac => ac.ListCartId == listCartId).ToListAsync();
        }

       public async Task<ListCart> GetListCartById(Guid listCartId)
       {
            return await _context.ListCarts.FirstOrDefaultAsync(lc => lc.Id == listCartId);
       }

        public async Task UpdateCartListArchiveStatus(ListCart listCart, bool isArchive)
        {
            listCart.IsArchived = isArchive;
            if (listCart.Carts != null)
            {
                foreach (var cart in listCart.Carts)
                {
                    cart.IsArchived = isArchive;
                }
            }
            _context.ListCarts.Update(listCart);
            await _context.SaveChangesAsync();
        }
        public async Task<List<ListCart>> GetListCartsByBoardIdAndIsArchive(Guid boardId, bool isArchive)
        {
            return await _context.ListCarts
          .Include(lc => lc.Carts) 
              .ThenInclude(c => c.ActivityCart) 
          .Include(lc => lc.ActivityListCarts) 
          .Where(lc => lc.BoardId == boardId && lc.IsArchived == isArchive)
          .AsNoTracking()
          .ToListAsync();
        }

        public async Task<List<ListCart>> SearchListCartByName(Guid boardId, string cartName, bool isArchive)
        {
            var searchTerm = cartName.ToLower();
            return await _context.ListCarts
         .Include(lc => lc.Carts) 
             .ThenInclude(c => c.ActivityCart) 
         .Include(lc => lc.ActivityListCarts) 
         .Where(lc => lc.BoardId == boardId &&
                      lc.IsArchived == isArchive &&
                      lc.Name.ToLower().Contains(searchTerm))
         .ToListAsync();
        }
    }

    
}
