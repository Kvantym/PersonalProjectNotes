using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IListCartService _listCartService;
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;

        public BoardService(IBoardRepository boardRepository, IListCartService listCartService, IUserService userService, IActivityService activityService)
        {
            _boardRepository = boardRepository;
            _listCartService = listCartService;
            _userService = userService;
            _activityService = activityService;
        }

        public async Task CreateBoard(CreateBoardRequest createBoardRequest, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            var board = new Board()
            {
                Name = createBoardRequest.Name,
                UserId = userId,
                CreatedAt = createBoardRequest.CreatedAt,
              //  Action = UserAction.Create,
            };
            await _boardRepository.Create(board);
            await _activityService.AddActivityToBoard(board.Id, UserAction.Create, userId);
        }

        public async Task DeleteBoard(Guid boardId, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            var board = await GetOrThrowBoard(boardId);

            if (board.UserId == userId)
            {
                await _boardRepository.Delete(board);
            }
            else
            {
                throw new NotFoundException("You do not have permission to delete this board");
            }            
        }

        public async Task<BoardResponse> GetBoard(Guid boardId, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            var board = await GetOrThrowBoard(boardId);
            if (board.UserId != userId)
            {
                throw new BadRequestException("You do not have permission to view this board");
            }

            var cartLists = await _listCartService.GetListCartsAsync(userId);

            var boardResponse = CreateBoardResponse(board, cartLists);

            return boardResponse;
        }

        public async Task<List<BoardResponse>> GetBoards(Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }

            var boards = await GetOrThrowBoardsByUserId(userId);

            var cartLists = await _listCartService.GetListCartsAsync(userId);

            var boardsResponse = boards.Select(board => CreateBoardResponse(board, cartLists)).ToList();
          
            return boardsResponse;
        }

        public async Task UpdateBoard(Guid boardId, UpdateBoardRequest updateBoardRequest, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            var board = await GetOrThrowBoard(boardId);
            var previousBoardState = new Board
            {
                Name = board.Name
            };

            if (board.UserId == userId)
            {
                board.Name = updateBoardRequest.Name;
                board.UpdatedAt = DateTime.Now;
            }

            await _boardRepository.Update(board);
            await _activityService.AddActivityToBoard(boardId, UserAction.Update, userId, previousBoardState);
        }

        public async Task<Board> GetOrThrowBoard(Guid boardId)
        {
            var board = await _boardRepository.GetBoard(boardId);
            if (board == null)
            {
                throw new NotFoundException($"Board with ID {boardId} not found");
            }
            return board;
        }

        public async Task<List<Board>> GetOrThrowBoardsByUserId(Guid userId)
        {
            var boards = await _boardRepository.GetBoards(userId);

            if (boards == null || !boards.Any())
            {
                return new List<Board>();
            }
            return boards;
        }

        public async Task<List<ActivityBoardResponse>> GetActivityBoardById(Guid boardId)
        {
            var board = await GetOrThrowBoard(boardId);

            return CreateActivityBoardResponse(board);
        }

        private List<ActivityBoardResponse> CreateActivityBoardResponse(Board board)
        {
            return board.ActivityBoards?.Select(a => new ActivityBoardResponse
            {
                Id = a.Id,
                Action = a.Action,
                ActivityInformation = a.ActivityInformation,
                UserId = a.UserId,
                ActivityTime = a.ActivityTime,
                BoardId = a.BoardId
            }).ToList();
        }

        private BoardResponse CreateBoardResponse(Board board, List<ListCartResponse> cartLists)
        {
            return new BoardResponse()
            {
                Id = board.Id,
                Name = board.Name,
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt,
                UserId = board.UserId,
                ActivityBoards = CreateActivityBoardResponse(board),
                ListCart = cartLists.Where(c => c.BoardId == board.Id).ToList()
            };
        }
    }
}
//208
