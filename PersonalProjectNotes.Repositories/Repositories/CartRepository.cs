using Microsoft.EntityFrameworkCore;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Repositories.Interfaces;

namespace PersonalProjectNotes.Repositories.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddActivity(ActivityCart activity)
        {
            _context.ActivitiesCarts.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task Create(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Cart cart)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        public async Task<Cart> GetCart(Guid cartId)
        {
            return await _context.Carts.Include(ac => ac.ActivityCart)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }
        public async Task<List<Cart>> GetCartsByCartList(Guid cartListId, bool isArchive)
        {
            return await _context.Carts.Where(c => c.ListCartId == cartListId && c.IsArchived == isArchive).ToListAsync();
        }


        //public async Task<List<Cart>> GetCarts(Guid userID)
        //{
        //    return await _context.Carts.Include(ac=> ac.ActivityCart).Where(c=> c.UserId== userID).ToListAsync();
        //}

        public async Task Update(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityCart>> GetActivityCart(Guid cartId)
        {
            return await _context.ActivitiesCarts.Where(ac => ac.CartId == cartId).ToListAsync();
        }
        public async Task UpdateCartArchiveStatus(Cart cart, bool isArchive)
        {
            cart.IsArchived = isArchive;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Cart>> GetCarts(Guid userId, bool isArchive)
        {
            return await _context.Carts.Include(ac => ac.ActivityCart).Where(c => c.UserId == userId && c.IsArchived == isArchive).ToListAsync();
        }

        //public async Task<List<Cart>> SearchCartByName(Guid ListCartId, string cartName, bool isArchive)
        //{
        //    var searchTerm = cartName.ToLower();
        //    var searchCart = await _context.Carts.Where(c => c.ListCartId == ListCartId && c.IsArchived == isArchive && c.Name.ToLower().Contains(searchTerm)).ToListAsync();
        //    return searchCart;
        //}

        public async Task<List<Cart>> GetCartWithFilter(Guid ListCartId, string searchTerm, bool isArchive, PriorityNote? priority, StatusNote? status, DateTime? DueDate, DateTime? CreatedAt)
        {
            var query = _context.Carts.Where(c => c.ListCartId == ListCartId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term));
            }

            if (priority.HasValue)
            {
                query = query.Where(c => c.PriorityNote == priority.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.StatusNote == status.Value);
            }

            if (DueDate.HasValue)
            {
                query = query.Where(c => c.DueDate.Date == DueDate.Value.Date);
            }

            if (CreatedAt.HasValue)
            {
                query = query.Where(c => c.CreatedAt.Date == CreatedAt.Value.Date);
            }

            return await query.ToListAsync();
        }
    }
}
