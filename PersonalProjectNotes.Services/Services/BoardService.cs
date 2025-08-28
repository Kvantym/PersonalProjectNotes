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
        private readonly IUserRepository _userRepository;

        public BoardService(IBoardRepository boardRepository, IUserRepository userRepository)
        {
            _boardRepository = boardRepository;
            _userRepository = userRepository;
        }

        public async Task AddActivityToBoard(Guid BoardID, UserAction Action, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
                throw  new NotFoundException($"User with ID {UserID} not found");
            }
            var board = await _boardRepository.GetBoard(BoardID);
            if (board == null)
            {
                throw new NotFoundException("Board not found");
            }
            if (board.UserId == UserID)
            {
                var Activity = new ActivityBoard()
                {
                    BoardId = BoardID,
                    Board = board,
                    Action = Action,
                    UserId = UserID,
                    ActivityInformation = $"{user.UserName} виконав дію {Action} з карткою {board.Name}"
                };
                await _boardRepository.AddActivity(Activity);
            }
        }

        public async Task CreateBoard(CreateBoardRequest createBoardRequest, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {UserID} not found");
            }
            var board = new Board()
            {
                Name = createBoardRequest.Name,
                UserId = UserID,
                CreatedAt = createBoardRequest.CreatedAt,
                Action = createBoardRequest.UserAction,

            };
            await _boardRepository.Create(board);
            await AddActivityToBoard(board.Id, UserAction.Create, UserID);
        }

        public async Task DeleteBoard(Guid BoardID, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
               throw new NotFoundException($"User with ID {UserID} not found");
            }
            var board = await _boardRepository.GetBoard(BoardID);
            if (board == null)
            {
                throw new NotFoundException($"Board with ID {BoardID} not found");
            }
            if (board.UserId == UserID)
            {
               await _boardRepository.Delete(board);
            }
            else
            {
                throw new NotFoundException("You do not have permission to delete this board");
            }
            await AddActivityToBoard(board.Id, UserAction.Delete, UserID);
        }

        public async Task<BoardResponse> GetBoard(Guid BoardID, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {UserID} not found");
            }

            var board = await _boardRepository.GetBoard(BoardID);
            if (board == null)
            {
                throw new NotFoundException($"Board with ID {BoardID} not found");
            }

            var boardResponse = new BoardResponse()
            {
                Name = board.Name,
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt,
                UserId = board.UserId,

                // Перетворення ActivityBoards
                ActivityBoards = board.ActivityBoards?.Select(a => new ActivityBoardResponse
                {
                    Id = a.Id,
                    Action = a.Action,
                    ActivityInformation = a.ActivityInformation,
                    UserId = a.UserId,
                    ActivityTime = a.ActivityTime,
                    BoardId = a.BoardId
                }).ToList(),

                // Перетворення ListCart разом з ActivityListCarts
                ListCart = board.ListCart?.Select(listCart => new ListCartResponse
                {
                    Name = listCart.Name,
                    BoardId = listCart.BoardId,
                    UserId = listCart.UserId,

                    // Carts
                    Carts = listCart.Carts?.Select(c => new CartResponse
                    {
                        Name = c.Name,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        DueDate = c.DueDate,
                        UserId = c.UserId,
                        PriorityNote = c.PriorityNote,
                        StatusNote = c.StatusNote,
                        Action = c.Action,
                        ListCartId = c.ListCartId,

                        ActivityCart = c.ActivityCart?.Select(ac => new ActivityCartResponse
                        {
                            Id = ac.Id,
                            Action = ac.Action,
                            ActivityInformation = ac.ActivityInformation,
                            UserId = ac.UserId,
                            ActivityTime = ac.ActivityTime,
                            CartId = ac.CartId
                        }).ToList()
                    }).ToList(),

                    // ActivityListCarts
                    ActivityListCarts = listCart.ActivityListCarts?.Select(alc => new ActivityListCartResponse
                    {
                        Id = alc.Id,
                        Action = alc.Action,
                        ActivityInformation = alc.ActivityInformation,
                        UserId = alc.UserId,
                        ActivityTime = alc.ActivityTime,
                        ListCartId = alc.ListCartId
                    }).ToList()

                }).ToList()
            };

            return boardResponse;
        }


        public async Task<List<BoardResponse>> GetBoards(Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {UserID} not found");
            }

            var boards = await _boardRepository.GetBoards(UserID);
            if (boards == null || !boards.Any())
            {
                throw new NotFoundException("No boards found for the user");
            }

            var boardsResponse = boards.Select(board => new BoardResponse()
            {
                Name = board.Name,
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt,
                UserId = board.UserId,

                ActivityBoards = board.ActivityBoards?.Select(a => new ActivityBoardResponse
                {
                    Id = a.Id,
                    Action = a.Action,
                    ActivityInformation = a.ActivityInformation,
                    UserId = a.UserId,
                    ActivityTime = a.ActivityTime,
                    BoardId = a.BoardId
                }).ToList(),

                ListCart = board.ListCart?.Select(listCart => new ListCartResponse
                {
                    Name = listCart.Name,
                    BoardId = listCart.BoardId,
                    UserId = listCart.UserId,

                    Carts = listCart.Carts?.Select(c => new CartResponse
                    {
                        Name = c.Name,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        DueDate = c.DueDate,
                        UserId = c.UserId,
                        PriorityNote = c.PriorityNote,
                        StatusNote = c.StatusNote,
                        Action = c.Action,
                        ListCartId = c.ListCartId,

                        ActivityCart = c.ActivityCart?.Select(ac => new ActivityCartResponse
                        {
                            Id = ac.Id,
                            Action = ac.Action,
                            ActivityInformation = ac.ActivityInformation,
                            UserId = ac.UserId,
                            ActivityTime = ac.ActivityTime,
                            CartId = ac.CartId
                        }).ToList()
                    }).ToList(),

                    ActivityListCarts = listCart.ActivityListCarts?.Select(alc => new ActivityListCartResponse
                    {
                        Id = alc.Id,
                        Action = alc.Action,
                        ActivityInformation = alc.ActivityInformation,
                        UserId = alc.UserId,
                        ActivityTime = alc.ActivityTime,
                        ListCartId = alc.ListCartId
                    }).ToList()

                }).ToList()

            }).ToList();

            return boardsResponse;
        }



        public async Task UpdateBoard(Guid BoardID, UpdateBoardRequest updateBoardRequest, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {UserID} not found");
            }
            var board = await _boardRepository.GetBoard(BoardID);
            if (board == null)
            {
                throw new NotFoundException($"Board with ID {BoardID} not found");
            }
            if (board.UserId == UserID)
            {
                board.Name = updateBoardRequest.Name;
                board.UpdatedAt = updateBoardRequest.UpdatedAt;
            }

            await _boardRepository.Update(board);
            await AddActivityToBoard(board.Id, UserAction.Update, UserID);
        }
    }
}
