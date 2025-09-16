using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            return await _context.Carts.Include(ac=> ac.ActivityCart)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }
        public async Task<List<Cart>> GetCartsByCartList(Guid cartListId)
        {
            return await _context.Carts.Where(c => c.ListCartId == cartListId).ToListAsync();
        }


        public async Task<List<Cart>> GetCarts(Guid userID)
        {
            return await _context.Carts.Include(ac=> ac.ActivityCart).Where(c=> c.UserId== userID).ToListAsync();
        }

        public async Task Update(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityCart>> GetActivityCart(Guid cartId)
        {
            return await _context.ActivitiesCarts.Where(ac => ac.CartId == cartId).ToListAsync();
        }
    }
}
