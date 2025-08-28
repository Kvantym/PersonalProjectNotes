using PersonalProjectNotes.Domain.Entities;

namespace PersonalProjectNotes.Repositories.Interfaces
{
    public interface IBoardRepository
    {
        public Task Create(Board Board);
        public Task Delete(Board Board);
        public Task Update(Board Board);
        public Task<Board> GetBoard(Guid BoardID);
        public Task<List<Board>> GetBoards(Guid UserID);
        public Task AddActivity(ActivityBoard activity);

    }
}
