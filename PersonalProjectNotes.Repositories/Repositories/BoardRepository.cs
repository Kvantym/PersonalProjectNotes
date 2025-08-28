using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace PersonalProjectNotes.Repositories.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly AppDbContext _context;

        public BoardRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddActivity(ActivityBoard activity)
        {
           _context.ActivitiesListBoards.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task Create(Board Board)
        {
            _context.Boards.Add(Board);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Board Board)
        {
            _context.Boards.Remove(Board);
            await _context.SaveChangesAsync();
        }

        public async Task<Board> GetBoard(Guid boardId)
        {
            return await _context.Boards
                .Include(b => b.ListCart)
                    .ThenInclude(lc => lc.Carts)
                        .ThenInclude(c => c.ActivityCart)
                .Include(b => b.ListCart)
                    .ThenInclude(lc => lc.ActivityListCarts)
                .Include(b => b.ActivityBoards)
                .FirstOrDefaultAsync(b => b.Id == boardId);
        }




        public async Task<List<Board>> GetBoards(Guid userId)
        {
            return await _context.Boards
     .Where(b => b.UserId == userId)
     .Include(b => b.ListCart) // ListCart у Board
         .ThenInclude(lc => lc.Carts) // Carts у ListCart
             .ThenInclude(c => c.ActivityCart) // ActivityCart у Cart
     .Include(b => b.ListCart) // ще раз підтягуємо ActivityListCarts
         .ThenInclude(lc => lc.ActivityListCarts)
     .Include(b => b.ActivityBoards) // ActivityBoards у Board
     .ToListAsync();

        }



        public async Task Update(Board Board)
        {
            _context.Boards.Update(Board);
            await _context.SaveChangesAsync();
        }
    }
}
