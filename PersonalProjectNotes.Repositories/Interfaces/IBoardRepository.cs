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
        public Task<List<ActivityBoard>> GetActivityBoardById(Guid boardId);
        public Task AddColloborator(Board board, Guid collaboratorId);
        public Task<List<ApplicationUser>> GetAllColloborators(Guid boardId);

        public Task DeleteUserFromBoard(Board board, ApplicationUser colobarator);
        public Task ToggleArchiveStatus(Board board, bool status);

        public Task<List<Board>> GetArchivedBoardsByUserIdAndArchiveStatus(Guid userId, bool isArchive);

        public Task<List<Board>> SearchBoardsByName(Guid userId, string boardName,bool isArchive);

    }
}
