using FluentAssertions;
using Moq;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.ListCart;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;

namespace PersonalProjectNotes.IntegrationTests.UnitTests;

public class ListCartServiceAdditionalTests
{
    private readonly Mock<IListCartRepository> _listCartRepositoryMock = new();
    private readonly Mock<ICartRepository> _cartRepositoryMock = new();
    private readonly Mock<ICartService> _cartServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IActivityService> _activityServiceMock = new();
    private readonly Mock<IBoardRepository> _boardRepositoryMock = new();
    private readonly ListCartService _service;

    public ListCartServiceAdditionalTests()
    {
        _service = new ListCartService(
            _listCartRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _cartServiceMock.Object,
            _userServiceMock.Object,
            _activityServiceMock.Object,
            _boardRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesListCartAndAddsActivities()
    {
        var userId = Guid.NewGuid();
        var boardId = Guid.NewGuid();

        await _service.CreateAsync(new CreateListCartRequest { Name = "Todo" }, userId, boardId);

        _listCartRepositoryMock.Verify(r => r.Create(It.Is<ListCart>(lc =>
            lc.Name == "Todo" &&
            lc.UserId == userId &&
            lc.BoardId == boardId)), Times.Once);
        _activityServiceMock.Verify(a =>
            a.AddActivityToCartList(It.IsAny<Guid>(), UserAction.Create, userId, null, null, null, null),
            Times.Once);
        _activityServiceMock.Verify(a =>
            a.AddActivityToBoard(boardId, UserAction.CreateListCart, userId, null, null, It.IsAny<ListCart>(), null),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeletesListAndItsCarts_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var listCart = new ListCart { Id = Guid.NewGuid(), UserId = userId, BoardId = Guid.NewGuid(), Name = "Todo" };
        var carts = new List<Cart>
        {
            new() { Id = Guid.NewGuid(), ListCartId = listCart.Id },
            new() { Id = Guid.NewGuid(), ListCartId = listCart.Id }
        };

        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);
        _listCartRepositoryMock.Setup(r => r.GetListCartsByListId(listCart.Id)).ReturnsAsync(carts);

        await _service.DeleteAsync(listCart.Id, userId);

        _cartRepositoryMock.Verify(r => r.Delete(It.IsAny<Cart>()), Times.Exactly(2));
        _listCartRepositoryMock.Verify(r => r.Delete(listCart), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToBoard(
            listCart.BoardId,
            UserAction.DeleteListCart,
            userId,
            null,
            listCart,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenListCartDoesNotExist()
    {
        var listCartId = Guid.NewGuid();
        _listCartRepositoryMock.Setup(r => r.GetListCart(listCartId)).ReturnsAsync((ListCart?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(listCartId, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBadRequest_WhenUserIsNotOwner()
    {
        var listCart = new ListCart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Todo" };
        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);
        _listCartRepositoryMock.Setup(r => r.GetListCartsByListId(listCart.Id)).ReturnsAsync(new List<Cart>());

        await Assert.ThrowsAsync<BadRequestException>(() => _service.DeleteAsync(listCart.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetListCartAsync_ReturnsListWithOnlyItsCarts()
    {
        var userId = Guid.NewGuid();
        var listCartId = Guid.NewGuid();
        var listCart = new ListCart { Id = listCartId, UserId = userId, BoardId = Guid.NewGuid(), Name = "Todo" };

        _listCartRepositoryMock.Setup(r => r.GetListCart(listCartId)).ReturnsAsync(listCart);
        _cartServiceMock.Setup(s => s.GetCarts(userId)).ReturnsAsync(new List<CartResponse>
        {
            new() { Name = "Mine", ListCartId = listCartId },
            new() { Name = "Other", ListCartId = Guid.NewGuid() }
        });

        var result = await _service.GetListCartAsync(listCartId);

        result.ListCartId.Should().Be(listCartId);
        result.Carts.Should().ContainSingle(c => c.Name == "Mine");
    }

    [Fact]
    public async Task GetListCartsAsync_ReturnsResponsesForUserLists()
    {
        var userId = Guid.NewGuid();
        var firstListId = Guid.NewGuid();

        _userServiceMock.Setup(s => s.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
        _listCartRepositoryMock.Setup(r => r.GetListCarts(userId)).ReturnsAsync(new List<ListCart>
        {
            new() { Id = firstListId, UserId = userId, BoardId = Guid.NewGuid(), Name = "Todo" },
            new() { Id = Guid.NewGuid(), UserId = userId, BoardId = Guid.NewGuid(), Name = "Done" }
        });
        _cartServiceMock.Setup(s => s.GetCarts(userId)).ReturnsAsync(new List<CartResponse>
        {
            new() { Name = "Task", ListCartId = firstListId }
        });

        var result = await _service.GetListCartsAsync(userId);

        result.Should().HaveCount(2);
        result.Single(r => r.ListCartId == firstListId).Carts.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesListAndAddsActivity_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var listCart = new ListCart { Id = Guid.NewGuid(), UserId = userId, BoardId = Guid.NewGuid(), Name = "Old" };
        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);

        await _service.UpdateAsync(listCart.Id, new UpdateListCartRequest { Name = "New" }, userId);

        listCart.Name.Should().Be("New");
        _listCartRepositoryMock.Verify(r => r.Update(listCart), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToCartList(
            listCart.Id,
            UserAction.Update,
            userId,
            It.Is<ListCart>(previous => previous.Name == "Old" && previous.BoardId == listCart.BoardId),
            null,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsBadRequest_WhenUserIsNotOwner()
    {
        var listCart = new ListCart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Old" };
        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateAsync(listCart.Id, new UpdateListCartRequest { Name = "New" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task MoveToBoard_UpdatesBoardIdAndAddsActivity()
    {
        var userId = Guid.NewGuid();
        var targetBoardId = Guid.NewGuid();
        var listCart = new ListCart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BoardId = Guid.NewGuid(),
            Name = "Todo",
            Board = new Board { Name = "Old board" }
        };
        var targetBoard = new Board { Id = targetBoardId, Name = "New board" };

        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);
        _boardRepositoryMock.Setup(r => r.GetBoard(targetBoardId)).ReturnsAsync(targetBoard);

        await _service.MoveToBoard(listCart.Id, targetBoardId, userId);

        listCart.BoardId.Should().Be(targetBoardId);
        _listCartRepositoryMock.Verify(r => r.Update(listCart), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToCartList(
            listCart.Id,
            UserAction.MoveToAtherBoard,
            userId,
            null,
            "Old board",
            "New board",
            null), Times.Once);
    }

    [Fact]
    public async Task GetThrowListsCartByUser_ThrowsNotFound_WhenRepositoryReturnsNull()
    {
        var userId = Guid.NewGuid();
        _listCartRepositoryMock.Setup(r => r.GetListCarts(userId)).ReturnsAsync((List<ListCart>?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetThrowListsCartByUser(userId));
    }

    [Fact]
    public async Task GetListCartByBoardId_ThrowsNotFound_WhenRepositoryReturnsNull()
    {
        var boardId = Guid.NewGuid();
        _listCartRepositoryMock.Setup(r => r.GetListCartsByBoardIdAndIsArchive(boardId, false))
            .ReturnsAsync((List<ListCart>?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetListCartByBoardId(boardId, false));
    }

    [Fact]
    public async Task GetListCartActivityByListId_MapsActivities()
    {
        var listCart = new ListCart
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Todo",
            ActivityListCarts = new List<ActivityListCart>
            {
                new() { Action = UserAction.Create, ActivityInformation = "created", ActivityTime = DateTime.UtcNow }
            }
        };
        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);

        var result = await _service.GetListCartActivityByListId(listCart.Id);

        result.Should().ContainSingle(a => a.Action == UserAction.Create && a.ActivityInformation == "created");
    }

    [Fact]
    public async Task ListCartExists_ReturnsTrue_WhenRepositoryReturnsTrue()
    {
        var listCartId = Guid.NewGuid();
        _listCartRepositoryMock.Setup(r => r.ExistsAsync(listCartId)).ReturnsAsync(true);

        var result = await _service.ListCartExists(listCartId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCartListArchiveStatus_ThrowsBadRequest_WhenUserIsNotOwner()
    {
        var listCart = new ListCart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _listCartRepositoryMock.Setup(r => r.GetListCart(listCart.Id)).ReturnsAsync(listCart);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateCartListArchiveStatus(Guid.NewGuid(), listCart.Id, true));
    }

    [Fact]
    public async Task SearchListCartByName_ReturnsBoardLists_WhenSearchTermIsEmpty()
    {
        var boardId = Guid.NewGuid();
        _listCartRepositoryMock.Setup(r => r.GetListCartsByBoardIdAndIsArchive(boardId, false))
            .ReturnsAsync(new List<ListCart> { new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Todo" } });

        var result = await _service.SearchListCartByName(boardId, "", false);

        result.Should().ContainSingle();
        _listCartRepositoryMock.Verify(r => r.SearchListCartByName(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
}
