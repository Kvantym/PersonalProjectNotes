using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IBoardService
    {
        public Task CreateBoard(CreateBoardRequest createBoardRequest, Guid UserID);
        public Task UpdateBoard(Guid BoardID, UpdateBoardRequest updateBoardRequest, Guid UserID);
        public Task DeleteBoard(Guid BoardID, Guid UserID);
        public Task<BoardResponse> GetBoard(Guid BoardID, Guid UserID);
        public Task<List<BoardResponse>> GetBoards(Guid UserID);
        public Task AddActivityToBoard(Guid BoardID, UserAction Action, Guid UserID);
    }
}
  