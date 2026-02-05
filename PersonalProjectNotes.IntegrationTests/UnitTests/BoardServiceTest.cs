using Microsoft.AspNetCore.Components;
using Moq;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Repositories.Repositories;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;

namespace PersonalProjectNotes.IntegrationTests.UnitTests
{
    public class BoardServiceTest
    {
        private readonly Mock<IBoardRepository> _boardRepositoryMock;
        private readonly Mock<IListCartService> _listCartServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IActivityService> _activityServiceMock;

        private readonly IBoardService _boardService;

        public BoardServiceTest()
        {
            _boardRepositoryMock = new Mock<IBoardRepository>();
            _listCartServiceMock = new Mock<IListCartService>();
            _userServiceMock = new Mock<IUserService>();
            _activityServiceMock = new Mock<IActivityService>();

            _boardService = new Services.Services.BoardService(
                _boardRepositoryMock.Object,
                _listCartServiceMock.Object,
                _userServiceMock.Object,
                _activityServiceMock.Object);
        }

        [Fact]
        public async Task CreateBoard_Should_CreatrBoard_and_AddActivity()
        {
            //arrange
            var userId = Guid.NewGuid();
            var request = new CreateBoardRequest
            {
                Name = "Test Board",
            };

            _userServiceMock.Setup(x => x.GetOrThrowUser(userId))
                .ReturnsAsync(new Domain.Entities.ApplicationUser { Id = userId, UserName = "testuser" });

            //act
            await _boardService.CreateBoard(request, userId);

            //assert
            _boardRepositoryMock.Verify(x => x.Create(It.Is<Board>(b => b.Name == "Test Board" && b.UserId == userId)), Times.Once);
            _activityServiceMock.Verify(x=>x.AddActivityToBoard(It.IsAny<Guid>(),UserAction.Create,userId,null,null,null), Times.Once);
        }

        [Fact]
        public async Task DeleteBoard_Shold_Delete_WhenUserIsOwner()
        {
            //arrange
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();
            var board = new Board
            {
                Id = boardId,
                Name = "Test Board",
                UserId = userId
            };

            _userServiceMock.Setup(x=>x.GetOrThrowUser(userId)).ReturnsAsync(new Domain.Entities.ApplicationUser { Id = userId, UserName = "testuser" });
            _boardRepositoryMock.Setup(x => x.GetBoard(boardId)).ReturnsAsync(board);

            //act
            await _boardService.DeleteBoard(boardId, userId);
            //assert
            _boardRepositoryMock.Verify(x => x.Delete(board), Times.Once);

        }
        
        [Fact]
        public async Task DeleteBoard_Shold_Delete_WhenUserIsNotOwner()
        {
            //arrange
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();
            var board = new Board
            {
                Id = boardId,
                Name = "Test Board",
                UserId = Guid.NewGuid(),
            };

            _userServiceMock.Setup(x => x.GetOrThrowUser(userId)).ReturnsAsync(new Domain.Entities.ApplicationUser { Id = userId });

            _boardRepositoryMock.Setup(x => x.GetBoard(boardId)).ReturnsAsync(board);

            //act & assert

            await Assert.ThrowsAsync<NotFoundException>(() => _boardService.DeleteBoard(boardId, userId));

            _boardRepositoryMock.Verify(r => r.Delete(It.IsAny<Board>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBoard_WhenUserIsOwner()
        {
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                UserName = "testuser"
            };
            var updateRequest = new UpdateBoardRequest
            {
                Name = "Updated Board Name"
            };
            var board = new Board
            {
                Id = boardId,
                Name = "Original Board Name",
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            };

            _userServiceMock.Setup(users => users.GetOrThrowUser(userId)).ReturnsAsync( user);

            _boardRepositoryMock.Setup(board=> board.GetBoard(boardId)).ReturnsAsync(board);
            //act
            await _boardService.UpdateBoard(board.Id, updateRequest, userId);
            //assert
            _boardRepositoryMock.Verify(r => r.Update(It.Is<Board>(b => b.Name == "Updated Board Name" && b.Id == boardId)), Times.Once);
        }
        [Fact]
        public async Task UpdateBoard_WhenUserIsNotOwner()
        {
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();

            var updateRequest = new UpdateBoardRequest
            {
                Name = "Not Owner Board Name"
            };
            var board = new Board
            {
                Id = boardId,
                Name = "Original Board Name",
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            };

            _userServiceMock.Setup(u=> u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId});

            _boardRepositoryMock.Setup(b=>b.GetBoard(boardId)).ReturnsAsync(board);

            await _boardService.UpdateBoard(board.Id, updateRequest, userId);

            _boardRepositoryMock.Verify(r => r.Update(It.Is<Board>(b => b.Name == "Original Board Name")), Times.Once);
        }
        [Fact]
        public async Task GetBoard_WhenBoardIsNotFound()
        {
            var boardId = Guid.NewGuid();

            _boardRepositoryMock.Setup(b=>b.GetBoard(boardId)).ReturnsAsync((Board?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _boardService.GetOrThrowBoard(boardId));
        }

        [Fact]
        public async Task GetBoardsByUserId_WhenBoardsIsNotFound()
        {
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u=> u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId});

            _boardRepositoryMock.Setup(b=>b.GetBoards(userId)).ReturnsAsync((List<Board>?)null);

            var result = await _boardService.GetOrThrowBoardsByUserId(userId);

            Assert.Empty(result);

        }
        [Fact]
        public async Task GetBoardsByUserId_WhenBoardsIsFound()
        {
            var userId = Guid.NewGuid();
            var boards = new List<Board>
            {
                new Board { Id = Guid.NewGuid(), Name = "Board 1", UserId = userId },
                new Board { Id = Guid.NewGuid(), Name = "Board 2", UserId = userId }
            };

            _userServiceMock.Setup(u=> u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId});

            _boardRepositoryMock.Setup(b=>b.GetBoards(userId)).ReturnsAsync(boards);

            var result = await _boardService.GetOrThrowBoardsByUserId(userId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetActivityBoardById_WhenBoardIsFound()
        {
            var boardId = Guid.NewGuid();

            var activityBoards = new List<ActivityBoard>
            {
                new ActivityBoard
                {
                    Id = Guid.NewGuid(),
                    BoardId = boardId,
                    Action = UserAction.Create,
                    UserId = Guid.NewGuid()
                },
                new ActivityBoard
                {
                    Id = Guid.NewGuid(),
                    BoardId = boardId,
                    Action = UserAction.Update,
                    UserId = Guid.NewGuid()
                }
            };
            var board = new Board
            {
                Id = boardId,
                Name = "Test Board",
                ActivityBoards = activityBoards
            };  

            _boardRepositoryMock.Setup(b=>b.GetBoard(boardId)).ReturnsAsync(board);

            var result = await _boardService.GetActivityBoardById(boardId);
            Assert.Equal(2, result.Count);
        }
        [Fact]
        public async Task GetActivityBoardById_WhenBoardIsNotFound()
        {
            var boardId = Guid.NewGuid();

            _boardRepositoryMock.Setup(b => b.GetBoard(boardId)).ReturnsAsync((Board?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _boardService.GetActivityBoardById(boardId));
        }

        [Fact]
        public async Task GetBoard_WhenBoardIsFound_And_UserIsOwner()
        {
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();

            var board = new Board
            {
                Id = boardId,
                Name = "Test Board",
                UserId = userId
            };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

            _boardRepositoryMock.Setup(b => b.GetBoard(boardId)).ReturnsAsync(board);

            _listCartServiceMock.Setup(l=> l.GetListCartsAsync(userId)).ReturnsAsync(new List<ListCartResponse>());

            var result = await _boardService.GetBoard(boardId, userId);

            Assert.Equal(boardId, result.Id);
        }
        [Fact]
        public async Task GetBoard_WhenBoardIsNotFound_And_UserIsOwner()
        {
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();

            var board = new Board
            {
                Id = boardId,
                Name = "Test Board",
                UserId = userId
            };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

            _boardRepositoryMock.Setup(b => b.GetBoard(boardId)).ReturnsAsync((Board?)null);

            await Assert.ThrowsAsync<NotFoundException>(()=>_boardService.GetBoard(boardId,userId));
        }

        [Fact]
        public async Task GetBoard_WhenBoardIsFound_And_UserIsNotOwner()
        {
            var userId = Guid.NewGuid();
            var boardId = Guid.NewGuid();

            var board = new Board
            {
                Id = boardId,
                Name = "Test Board",
                UserId = Guid.NewGuid()
            };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

            _boardRepositoryMock.Setup(b => b.GetBoard(boardId)).ReturnsAsync(board);

            await Assert.ThrowsAsync<BadRequestException>(()=> _boardService.GetBoard(boardId, userId));
        }

        [Fact]
        public async Task GetBoards_WhenBoardsIsNotFound()
        {
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u=>u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId});

            _boardRepositoryMock.Setup(b=>b.GetBoards(userId)).ReturnsAsync(new List<Board>());

            var result = await _boardService.GetBoards(userId);
            Assert.Empty(result);

        }

        [Fact]
        public async Task GetBoards_WhenBoardsIsFound()
        {
            var userId = Guid.NewGuid();

            var boards = new List<Board>
            {
                new Board { Id = Guid.NewGuid(), Name = "Board 1", UserId = userId },
                new Board { Id = Guid.NewGuid(), Name = "Board 2", UserId = userId }
            };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

            _boardRepositoryMock.Setup(b => b.GetBoards(userId)).ReturnsAsync(boards);

            _listCartServiceMock.Setup(l=>l.GetListCartsAsync(userId)).ReturnsAsync(new List<ListCartResponse>());  

            var result = await _boardService.GetBoards(userId);

            Assert.Equal(2, result.Count);
        }    

    }
}
