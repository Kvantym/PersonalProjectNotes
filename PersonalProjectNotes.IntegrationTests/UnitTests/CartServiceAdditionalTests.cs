using FluentAssertions;
using Moq;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Cart;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;

namespace PersonalProjectNotes.IntegrationTests.UnitTests;

public class CartServiceAdditionalTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock = new();
    private readonly Mock<IListCartRepository> _listCartRepositoryMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IActivityService> _activityServiceMock = new();
    private readonly CartService _service;

    public CartServiceAdditionalTests()
    {
        _service = new CartService(
            _cartRepositoryMock.Object,
            _listCartRepositoryMock.Object,
            _userServiceMock.Object,
            _activityServiceMock.Object);
    }

    [Fact]
    public async Task CreateCart_CreatesCartAndAddsActivity()
    {
        var userId = Guid.NewGuid();
        var listCartId = Guid.NewGuid();
        var request = new CreateCartRequest
        {
            Name = "Task",
            Description = "Description",
            DueDate = DateTime.UtcNow.AddDays(2),
            PriorityNote = PriorityNote.High,
            StatusNote = StatusNote.Published
        };

        await _service.CreateCart(request, userId, listCartId);

        _cartRepositoryMock.Verify(r => r.Create(It.Is<Cart>(c =>
            c.Name == request.Name &&
            c.Description == request.Description &&
            c.UserId == userId &&
            c.ListCartId == listCartId &&
            c.PriorityNote == PriorityNote.High &&
            c.StatusNote == StatusNote.Published)), Times.Once);
        _activityServiceMock.Verify(a =>
            a.AddActivityToCart(It.IsAny<Guid>(), UserAction.Create, userId, null, null, null), Times.Once);
    }

    [Fact]
    public async Task CreateCart_ThrowsBadRequest_WhenListCartIdIsEmpty()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.CreateCart(new CreateCartRequest { Name = "Task" }, Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public async Task DeleteCart_DeletesCartAndAddsListActivity_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, ListCartId = Guid.NewGuid(), Name = "Task" };
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);

        await _service.DeleteCart(cart.Id, userId);

        _activityServiceMock.Verify(a => a.AddActivityToCartList(
            cart.ListCartId,
            UserAction.DeleteCartWithCartList,
            userId,
            null,
            null,
            null,
            cart), Times.Once);
        _cartRepositoryMock.Verify(r => r.Delete(cart), Times.Once);
    }

    [Fact]
    public async Task DeleteCart_ThrowsBadRequest_WhenUserIsNotOwner()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ListCartId = Guid.NewGuid() };
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);

        await Assert.ThrowsAsync<BadRequestException>(() => _service.DeleteCart(cart.Id, Guid.NewGuid()));
        _cartRepositoryMock.Verify(r => r.Delete(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task GetCart_ReturnsMappedCartResponse()
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ListCartId = Guid.NewGuid(),
            Name = "Task",
            Description = "Text",
            PriorityNote = PriorityNote.Medium,
            StatusNote = StatusNote.Draft,
            ActivityCart = new List<ActivityCart>()
        };
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);

        var result = await _service.GetCart(cart.Id);

        result.Name.Should().Be(cart.Name);
        result.Description.Should().Be(cart.Description);
        result.ListCartId.Should().Be(cart.ListCartId);
    }

    [Fact]
    public async Task GetCart_ThrowsNotFound_WhenCartDoesNotExist()
    {
        var cartId = Guid.NewGuid();
        _cartRepositoryMock.Setup(r => r.GetCart(cartId)).ReturnsAsync((Cart?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCart(cartId));
    }

    [Fact]
    public async Task GetCarts_ReturnsEmptyList_WhenUserHasNoCarts()
    {
        var userId = Guid.NewGuid();
        _cartRepositoryMock.Setup(r => r.GetCarts(userId, false)).ReturnsAsync(new List<Cart>());

        var result = await _service.GetCarts(userId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateCart_UpdatesFieldsAndAddsActivity_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var newListId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ListCartId = Guid.NewGuid(),
            Name = "Old",
            Description = "Old description",
            PriorityNote = PriorityNote.Low,
            StatusNote = StatusNote.Draft,
            DueDate = DateTime.UtcNow
        };
        var request = new UpdateCartRequest
        {
            Name = "New",
            Description = "New description",
            DueDate = DateTime.UtcNow.AddDays(5),
            ListCartId = newListId,
            PriorityNote = PriorityNote.High,
            StatusNote = StatusNote.Done
        };

        _userServiceMock.Setup(s => s.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);
        _listCartRepositoryMock.Setup(r => r.ExistsAsync(newListId)).ReturnsAsync(true);

        await _service.UpdateCart(cart.Id, request, userId);

        cart.Name.Should().Be("New");
        cart.Description.Should().Be("New description");
        cart.ListCartId.Should().Be(newListId);
        cart.PriorityNote.Should().Be(PriorityNote.High);
        cart.StatusNote.Should().Be(StatusNote.Done);
        _cartRepositoryMock.Verify(r => r.Update(cart), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToCart(
            cart.Id,
            UserAction.Update,
            userId,
            It.Is<Cart>(previous => previous.Name == "Old" && previous.Description == "Old description"),
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task UpdateCart_ThrowsInvalidOperation_WhenTargetListDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, ListCartId = Guid.NewGuid() };

        _userServiceMock.Setup(s => s.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);
        _listCartRepositoryMock.Setup(r => r.ExistsAsync(listId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateCart(cart.Id, new UpdateCartRequest { ListCartId = listId }, userId));
    }

    [Fact]
    public async Task MoveToCardList_ChangesListAndAddsActivity()
    {
        var userId = Guid.NewGuid();
        var oldListId = Guid.NewGuid();
        var newListId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, ListCartId = oldListId };
        var previousList = new ListCart { Id = oldListId, Name = "Todo" };
        var targetList = new ListCart { Id = newListId, Name = "Done" };

        _userServiceMock.Setup(s => s.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);
        _listCartRepositoryMock.Setup(r => r.GetListCart(oldListId)).ReturnsAsync(previousList);
        _listCartRepositoryMock.Setup(r => r.GetListCart(newListId)).ReturnsAsync(targetList);

        await _service.MoveToCardList(cart.Id, newListId, userId);

        cart.ListCartId.Should().Be(newListId);
        _cartRepositoryMock.Verify(r => r.Update(cart), Times.Once);
        _activityServiceMock.Verify(a => a.AddActivityToCart(
            cart.Id,
            UserAction.MoveToAtherCartList,
            userId,
            It.Is<Cart>(previous => previous.ListCartId == oldListId),
            "Todo",
            "Done"), Times.Once);
    }

    [Fact]
    public async Task MoveToCardList_ThrowsBadRequest_WhenUserIsNotOwner()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ListCartId = Guid.NewGuid() };
        _userServiceMock.Setup(s => s.GetOrThrowUser(It.IsAny<Guid>())).ReturnsAsync(new ApplicationUser());
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.MoveToCardList(cart.Id, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetCartsByListCart_ReturnsOnlyRepositoryItems_WhenItemsExist()
    {
        var listId = Guid.NewGuid();
        _cartRepositoryMock.Setup(r => r.GetCartsByCartList(listId, false)).ReturnsAsync(new List<Cart>
        {
            new() { Id = Guid.NewGuid(), ListCartId = listId },
            new() { Id = Guid.NewGuid(), ListCartId = listId }
        });

        var result = await _service.GetCartsByListCart(listId, false);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCartsByListCart_ReturnsEmptyList_WhenRepositoryReturnsNull()
    {
        var listId = Guid.NewGuid();
        _cartRepositoryMock.Setup(r => r.GetCartsByCartList(listId, true)).ReturnsAsync((List<Cart>?)null);

        var result = await _service.GetCartsByListCart(listId, true);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivityCart_ThrowsNotFound_WhenActivityIsMissing()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ActivityCart = new List<ActivityCart>() };
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);
        _cartRepositoryMock.Setup(r => r.GetActivityCart(cart.Id)).ReturnsAsync((List<ActivityCart>?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetActivityCart(cart.Id));
    }

    [Fact]
    public async Task UpdateCartArchiveStatus_UpdatesArchive_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        _cartRepositoryMock.Setup(r => r.GetCart(cart.Id)).ReturnsAsync(cart);

        await _service.UpdateCartArchiveStatus(userId, cart.Id, true);

        _cartRepositoryMock.Verify(r => r.UpdateCartArchiveStatus(cart, true), Times.Once);
    }

    [Fact]
    public async Task SearchCartsByFilter_TrimsSearchTermBeforeRepositoryCall()
    {
        var listId = Guid.NewGuid();
        _cartRepositoryMock.Setup(r => r.GetCartWithFilter(
                listId, "task", false, PriorityNote.Low, StatusNote.Draft, null, null))
            .ReturnsAsync(new List<Cart>());

        await _service.SearchCartsByFilter(listId, "  task  ", false, PriorityNote.Low, StatusNote.Draft, null, null);

        _cartRepositoryMock.Verify(r => r.GetCartWithFilter(
            listId, "task", false, PriorityNote.Low, StatusNote.Draft, null, null), Times.Once);
    }
}
