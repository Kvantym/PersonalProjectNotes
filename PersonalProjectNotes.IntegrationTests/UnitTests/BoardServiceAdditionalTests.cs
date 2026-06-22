using FluentAssertions;
using Moq;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;

namespace PersonalProjectNotes.IntegrationTests.UnitTests;

public class BoardServiceAdditionalTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock = new();
    private readonly Mock<IListCartService> _listCartServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IActivityService> _activityServiceMock = new();
    private readonly BoardService _service;

    public BoardServiceAdditionalTests()
    {
        _service = new BoardService(
            _boardRepositoryMock.Object,
            _listCartServiceMock.Object,
            _userServiceMock.Object,
            _activityServiceMock.Object);
    }

    [Fact]
    public async Task CreateBoard_CreatesBoardWithNotArchivedStatus()
    {
        var userId = Guid.NewGuid();
        var request = new CreateBoardRequest { Name = "Diploma" };

        await _service.CreateBoard(request, userId);

        _boardRepositoryMock.Verify(r => r.Create(It.Is<Board>(b =>
            b.Name == request.Name &&
            b.UserId == userId &&
            b.CreatedAt == request.CreatedAt &&
            b.IsArchived == false)), Times.Once);
    }

    [Fact]
    public async Task CreateBoard_AddsCreateActivity()
    {
        var userId = Guid.NewGuid();

        await _service.CreateBoard(new CreateBoardRequest { Name = "Board" }, userId);

        _activityServiceMock.Verify(a =>
            a.AddActivityToBoard(It.IsAny<Guid>(), UserAction.Create, userId, null, null, null, null),
            Times.Once);
    }

    [Fact]
    public async Task DeleteBoard_DeletesBoard_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var board = new Board { Id = Guid.NewGuid(), UserId = userId, Name = "Owner board" };
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await _service.DeleteBoard(board.Id, userId);

        _boardRepositoryMock.Verify(r => r.Delete(board), Times.Once);
    }

    [Fact]
    public async Task DeleteBoard_ThrowsNotFound_WhenBoardDoesNotExist()
    {
        var boardId = Guid.NewGuid();
        _boardRepositoryMock.Setup(r => r.GetBoard(boardId)).ReturnsAsync((Board?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteBoard(boardId, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteBoard_ThrowsNotFound_WhenUserIsNotOwner()
    {
        var board = new Board { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Private board" };
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteBoard(board.Id, Guid.NewGuid()));
        _boardRepositoryMock.Verify(r => r.Delete(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task GetBoard_ReturnsBoardWithOnlyItsListCarts_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var otherBoardId = Guid.NewGuid();
        var board = new Board { Id = boardId, UserId = userId, Name = "Main board" };

        _boardRepositoryMock.Setup(r => r.GetBoard(boardId)).ReturnsAsync(board);
        _listCartServiceMock.Setup(s => s.GetListCartsAsync(userId)).ReturnsAsync(new List<ListCartResponse>
        {
            new() { ListCartId = Guid.NewGuid(), BoardId = boardId, Name = "Todo", UserId = userId },
            new() { ListCartId = Guid.NewGuid(), BoardId = otherBoardId, Name = "Other", UserId = userId }
        });

        var result = await _service.GetBoard(boardId, userId);

        result.Id.Should().Be(boardId);
        result.ListCart.Should().ContainSingle();
        result.ListCart.Single().Name.Should().Be("Todo");
    }

    [Fact]
    public async Task GetBoard_ReturnsBoard_WhenUserIsCollaborator()
    {
        var ownerId = Guid.NewGuid();
        var collaboratorId = Guid.NewGuid();
        var board = new Board
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Name = "Shared board",
            Collaborators = new List<Guid> { collaboratorId }
        };

        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);
        _listCartServiceMock.Setup(s => s.GetListCartsAsync(collaboratorId)).ReturnsAsync(new List<ListCartResponse>());

        var result = await _service.GetBoard(board.Id, collaboratorId);

        result.Name.Should().Be("Shared board");
    }

    [Fact]
    public async Task GetBoard_ThrowsBadRequest_WhenUserHasNoAccess()
    {
        var board = new Board { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "No access" };
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await Assert.ThrowsAsync<BadRequestException>(() => _service.GetBoard(board.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateBoard_ChangesNameAndAddsActivity_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var board = new Board { Id = Guid.NewGuid(), UserId = userId, Name = "Old" };
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await _service.UpdateBoard(board.Id, new UpdateBoardRequest { Name = "New" }, userId);

        board.Name.Should().Be("New");
        _boardRepositoryMock.Verify(r => r.Update(board), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToBoard(
            board.Id,
            UserAction.Update,
            userId,
            It.Is<Board>(b => b.Name == "Old"),
            null,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task UpdateBoard_DoesNotChangeName_WhenUserIsNotOwner()
    {
        var board = new Board { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Old" };
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await _service.UpdateBoard(board.Id, new UpdateBoardRequest { Name = "New" }, Guid.NewGuid());

        board.Name.Should().Be("Old");
        _boardRepositoryMock.Verify(r => r.Update(board), Times.Once);
    }

    [Fact]
    public async Task GetOrThrowBoardsByUserId_ReturnsEmptyList_WhenRepositoryReturnsNull()
    {
        var userId = Guid.NewGuid();
        _boardRepositoryMock.Setup(r => r.GetArchivedBoardsByUserIdAndArchiveStatus(userId, false))
            .ReturnsAsync((List<Board>?)null);

        var result = await _service.GetOrThrowBoardsByUserId(userId, false);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivityBoardById_MapsActivityResponses()
    {
        var boardId = Guid.NewGuid();
        var activity = new ActivityBoard
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = Guid.NewGuid(),
            Action = UserAction.Create,
            ActivityInformation = "created",
            ActivityTime = DateTime.UtcNow
        };

        _boardRepositoryMock.Setup(r => r.GetBoard(boardId)).ReturnsAsync(new Board
        {
            Id = boardId,
            Name = "Board",
            ActivityBoards = new List<ActivityBoard> { activity }
        });

        var result = await _service.GetActivityBoardById(boardId);

        result.Should().ContainSingle(a => a.Id == activity.Id && a.Action == UserAction.Create);
    }

    [Fact]
    public async Task AddColloborator_AddsUserByName_WhenUserExists()
    {
        var ownerId = Guid.NewGuid();
        var collaborator = new ApplicationUser { Id = Guid.NewGuid(), UserName = "max" };
        var board = new Board { Id = Guid.NewGuid(), UserId = ownerId, Name = "Board" };

        _userServiceMock.Setup(s => s.GetOrThrowUser(ownerId)).ReturnsAsync(new ApplicationUser { Id = ownerId });
        _userServiceMock.Setup(s => s.GetUserByName("max")).ReturnsAsync(collaborator);
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await _service.AddColloborator(board.Id, "max", ownerId);

        _boardRepositoryMock.Verify(r => r.AddColloborator(board, collaborator.Id), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToBoard(
            board.Id,
            UserAction.AddUser,
            ownerId,
            null,
            null,
            null,
            collaborator), Times.Once);
    }

    [Fact]
    public async Task AddColloborator_ThrowsNotFound_WhenIdentifierDoesNotMatchUser()
    {
        var ownerId = Guid.NewGuid();
        _userServiceMock.Setup(s => s.GetOrThrowUser(ownerId)).ReturnsAsync(new ApplicationUser { Id = ownerId });
        _userServiceMock.Setup(s => s.GetUserByName("missing")).ReturnsAsync((ApplicationUser?)null);
        _userServiceMock.Setup(s => s.GetUserByEmail("missing")).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddColloborator(Guid.NewGuid(), "missing", ownerId));
    }

    [Fact]
    public async Task AddColloborator_ThrowsBadRequest_WhenUserAlreadyCollaborator()
    {
        var ownerId = Guid.NewGuid();
        var collaborator = new ApplicationUser { Id = Guid.NewGuid(), UserName = "max" };
        var board = new Board
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Collaborators = new List<Guid> { collaborator.Id }
        };

        _userServiceMock.Setup(s => s.GetOrThrowUser(ownerId)).ReturnsAsync(new ApplicationUser { Id = ownerId });
        _userServiceMock.Setup(s => s.GetUserByName("max")).ReturnsAsync(collaborator);
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await Assert.ThrowsAsync<BadRequestException>(() => _service.AddColloborator(board.Id, "max", ownerId));
    }

    [Fact]
    public async Task ToggleArchiveStatus_ArchivesOwnerBoardAndAddsActivity()
    {
        var userId = Guid.NewGuid();
        var board = new Board { Id = Guid.NewGuid(), UserId = userId };
        _boardRepositoryMock.Setup(r => r.GetBoard(board.Id)).ReturnsAsync(board);

        await _service.ToggleArchiveStatus(userId, board.Id, true);

        _boardRepositoryMock.Verify(r => r.ToggleArchiveStatus(board, true), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToBoard(
            board.Id, UserAction.Archived, userId, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task SearchBoardsByName_ReturnsAllBoards_WhenSearchTermIsEmpty()
    {
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(s => s.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
        _boardRepositoryMock.Setup(r => r.GetArchivedBoardsByUserIdAndArchiveStatus(userId, false))
            .ReturnsAsync(new List<Board> { new() { Id = Guid.NewGuid(), UserId = userId, Name = "Any" } });
        _listCartServiceMock.Setup(s => s.GetListCartsAsync(userId)).ReturnsAsync(new List<ListCartResponse>());

        var result = await _service.SearchBoardsByName(userId, "", false);

        result.Should().ContainSingle();
        _boardRepositoryMock.Verify(r => r.SearchBoardsByName(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
}
