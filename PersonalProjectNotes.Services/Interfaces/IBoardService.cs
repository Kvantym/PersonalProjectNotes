using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Domain.Response;

namespace PersonalProjectNotes.Services.Interfaces
{
    public interface IBoardService
    {
        public Task CreateBoard(CreateBoardRequest createBoardRequest, Guid userId);
        public Task UpdateBoard(Guid boardId, UpdateBoardRequest updateBoardRequest, Guid userId);
        public Task DeleteBoard(Guid boardId, Guid userId);
        public Task<BoardResponse> GetBoard(Guid boardId, Guid userId);
        public Task<List<BoardResponse>> GetBoards(Guid userId);
     //   public Task AddActivityToBoard(Guid boardId, UserAction userAction, Guid userId, Board? previousBoardState = null, ListCart? deleteListCart = null);
        public Task<Board> GetOrThrowBoard(Guid boardId);
        public Task<List<Board>> GetOrThrowBoardsByUserId(Guid userId);
        public Task<List<ActivityBoardResponse>> GetActivityBoardById(Guid boardId);

        public Task AddColloborator(Guid boardId, string identifier, Guid whoAddColoborationId);
        public Task<List<UserResponse>> GetAllColloborators(Guid boardId);

        public Task DeleteUserFromBoardIfUserIsOwner(Guid boardId, Guid ownerId, string collaborationName);

        public Task RemoveColoboratorFromBoard(Guid boardId, Guid collaborationId);
    }
}
