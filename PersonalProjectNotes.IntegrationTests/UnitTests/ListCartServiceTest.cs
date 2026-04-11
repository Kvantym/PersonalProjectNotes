//using Moq;
//using PersonalProjectNotes.Domain.Entities;
//using PersonalProjectNotes.Domain.Enums;
//using PersonalProjectNotes.Domain.Request.ListCart;
//using PersonalProjectNotes.Repositories.Interfaces;
//using PersonalProjectNotes.Services.Exceptions;
//using PersonalProjectNotes.Services.Interfaces;
//using PersonalProjectNotes.Services.Services;

//namespace PersonalProjectNotes.IntegrationTests.UnitTests
//{
//    public class ListCartServiceTest
//    {
//        private readonly Mock<IListCartRepository> _listCartRepositoryMock;
//        private readonly Mock<ICartRepository> _cartRepositoryMock;
//        private readonly Mock<ICartService> _cartServiceMock;
//        private readonly Mock<IUserService> _userServiceMock;
//        private readonly Mock<IActivityService> _activityServiceMock;
//        private readonly Mock<IBoardRepository> _boardRepositoryMock;

//        private readonly IListCartService _listCartService;

//        public ListCartServiceTest()
//        {
//            _listCartRepositoryMock = new Mock<IListCartRepository>();
//            _cartRepositoryMock = new Mock<ICartRepository>();
//            _cartServiceMock = new Mock<ICartService>();
//            _userServiceMock = new Mock<IUserService>();
//            _activityServiceMock = new Mock<IActivityService>();
//            _boardRepositoryMock = new Mock<IBoardRepository>();

//            _listCartService = new ListCartService(
//                _listCartRepositoryMock.Object,
//                _cartRepositoryMock.Object,
//                _cartServiceMock.Object,
//                _userServiceMock.Object,
//                _activityServiceMock.Object,
//                _boardRepositoryMock.Object
//            );
//        }

//        [Fact]
//        public async Task CreateAsync_ShouldCreateListCartAndAddActivities()
//        {
//            var userId = Guid.NewGuid();
//            var boardId = Guid.NewGuid();

//            var request = new CreateListCartRequest
//            {
//                Name = "Test List Cart"
//            };

//            _listCartRepositoryMock.Setup(cl => cl.Create(It.IsAny<ListCart>())).Returns(Task.CompletedTask);

//            await _listCartService.CreateAsync(request, userId, boardId);

//            _listCartRepositoryMock.Verify(cl => cl.Create(It.Is<ListCart>(lc => lc.Name == "Test List Cart" && lc.UserId == userId && lc.BoardId == boardId)), Times.Once);

//            _activityServiceMock.Verify(a => a.AddActivityToCartList(It.IsAny<Guid>(), UserAction.Create, userId, null, null, null, null), Times.Once);

//            _activityServiceMock.Verify(a => a.AddActivityToBoard(boardId, UserAction.CreateListCart, userId, null, null, It.IsAny<ListCart>()), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteAsync_WhenUserIsOwner()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();

//            var cartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = userId,
//                Name = "Test List Cart",
//                BoardId = Guid.NewGuid()
//            };
//            var carts = new List<Cart>
//            {
//                new Cart { Id = Guid.NewGuid(), ListCartId = cartListId, Name = "Cart 1", UserId = userId },
//                new Cart { Id = Guid.NewGuid(), ListCartId = cartListId, Name = "Cart 2", UserId = userId }
//            };
//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
//            _listCartRepositoryMock.Setup(lc => lc.GetListCart(cartListId)).ReturnsAsync(cartList);
//            _listCartRepositoryMock.Setup(c => c.GetListCartsByListId(cartListId)).ReturnsAsync(carts);
//            _cartRepositoryMock.Setup(c => c.Delete(It.IsAny<Cart>())).Returns(Task.CompletedTask);
//            _listCartRepositoryMock.Setup(lc => lc.Delete(cartList)).Returns(Task.CompletedTask);


//            await _listCartService.DeleteAsync(cartListId, userId);

//            _listCartRepositoryMock.Verify(c => c.GetListCartsByListId(cartListId), Times.Once);
//            _cartRepositoryMock.Verify(c => c.Delete(It.IsAny<Cart>()), Times.Exactly(carts.Count));
//            _listCartRepositoryMock.Verify(lc => lc.GetListCart(cartListId), Times.Once);
//            _activityServiceMock.Verify(a => a.AddActivityToBoard(cartList.BoardId, UserAction.DeleteListCart, userId, null, cartList, null), Times.Once);
//            _listCartRepositoryMock.Verify(lc => lc.Delete(cartList), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteAsync_WhenUserIsNotOwner()
//        {
//            var userId = Guid.NewGuid();

//            var cartListId = Guid.NewGuid();

//            var cartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = Guid.NewGuid(),
//                Name = "Test List Cart",
//                BoardId = Guid.NewGuid()
//            };

//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

//            _listCartRepositoryMock.Setup(lc => lc.GetListCart(cartListId)).ReturnsAsync(cartList);
//            _listCartRepositoryMock.Setup(c => c.GetListCartsByListId(cartListId)).ReturnsAsync(new List<Cart>());

//            await Assert.ThrowsAsync<BadRequestException>(() => _listCartService.DeleteAsync(cartListId, userId));
//        }

//        [Fact]
//        public async Task DeleteAsync_WhenCartListIsExsist()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();

//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

//            _listCartRepositoryMock.Setup(cl => cl.GetListCart(cartListId)).ReturnsAsync((ListCart?)null);

//            await Assert.ThrowsAsync<NotFoundException>(() => _listCartService.DeleteAsync(cartListId, userId));
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldUpdateListCartAndAddActivities_WhenUserIsOwner()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();

//            var existingCartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = userId,
//                Name = "Original List Cart",
//                BoardId = Guid.NewGuid(),
//            };

//            var previousCartListState = new ListCart
//            {
//                Name = existingCartList.Name,
//                BoardId = existingCartList.BoardId,
//            };

//            var updateRequest = new UpdateListCartRequest
//            {
//                Name = "Updated List Cart"
//            };

//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

//            _listCartRepositoryMock.Setup(cl => cl.GetListCart(cartListId)).ReturnsAsync(existingCartList);
//            _activityServiceMock.Setup(a => a.AddActivityToCartList(It.IsAny<Guid>(), It.IsAny<UserAction>(), It.IsAny<Guid>(), It.IsAny<ListCart?>(), null, null, null)).Returns(Task.CompletedTask);

//            _listCartRepositoryMock.Setup(cl => cl.Update(It.IsAny<ListCart>())).Returns(Task.CompletedTask);

//            await _listCartService.UpdateAsync(cartListId, updateRequest, userId);


//            _listCartRepositoryMock.Verify(cl => cl.Update(It.Is<ListCart>(lc => lc.Id == cartListId && lc.Name == "Updated List Cart")), Times.Once);
//            _activityServiceMock.Verify(a => a.AddActivityToCartList(cartListId, UserAction.Update, userId, It.Is<ListCart>(prev => prev.Name == "Original List Cart" && prev.BoardId == existingCartList.BoardId), null, null, null));
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldUpdateListCartAndAddActivities_WhenCartListIsNotFound()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();

//            var existingCartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = userId,
//                Name = "Original List Cart",
//                BoardId = Guid.NewGuid(),
//            };

//            _listCartRepositoryMock.Setup(cl => cl.GetListCart(cartListId)).ReturnsAsync((ListCart?)null);
//            var updateRequest = new UpdateListCartRequest
//            {
//                Name = "Updated List Cart"
//            };

//            await Assert.ThrowsAsync<NotFoundException>(() => _listCartService.UpdateAsync(cartListId, updateRequest, userId));
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldUpdateListCartAndAddActivities_WhenUserIsNotOwner()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();

//            var cartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = Guid.NewGuid(),
//                Name = "Original List Cart",
//                BoardId = Guid.NewGuid(),
//            };
//            var updateRequest = new UpdateListCartRequest
//            {
//                Name = "Updated List Cart"
//            };

//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });
//            _listCartRepositoryMock.Setup(cl => cl.GetListCart(cartListId)).ReturnsAsync(cartList);

//            await Assert.ThrowsAsync<BadRequestException>(() => _listCartService.UpdateAsync(cartListId, updateRequest, userId));
//        }

//        [Fact]
//        public async Task MoveToBoard_ShouldMoveListCartAndAddActivities_WhenUserIsOwner()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();
//            var boardId = Guid.NewGuid();

//            var cartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = userId,
//                Name = "Test List Cart",
//                BoardId = Guid.NewGuid(),
//                Board = new Board { Id = Guid.NewGuid(), Name = "Original Board" }
//            };

//            var targetBoard = new Board
//            {
//                Id = boardId,
//                Name = "Target Board",
//            };

//            var carenttListCart = cartList;

//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

//            _listCartRepositoryMock.Setup(cl => cl.GetListCart(cartListId)).ReturnsAsync(cartList);
//            _boardRepositoryMock.Setup(b => b.GetBoard(boardId)).ReturnsAsync(targetBoard);

//            cartList.BoardId = boardId;

//            _listCartRepositoryMock.Setup(cl => cl.Update(It.IsAny<ListCart>())).Returns(Task.CompletedTask);
//            _activityServiceMock.Setup(a => a.AddActivityToCartList(cartListId, UserAction.MoveToAtherBoard, userId, null, carenttListCart.Board.Name, targetBoard.Name, null)).Returns(Task.CompletedTask);

//            await _listCartService.MoveToBoard(cartListId, boardId, userId);

//            _listCartRepositoryMock.Verify(cl => cl.Update(It.Is<ListCart>(lc => lc.BoardId == boardId)), Times.Once);
//            _activityServiceMock.Verify(a => a.AddActivityToCartList(cartListId, UserAction.MoveToAtherBoard, userId, null, carenttListCart.Board.Name, targetBoard.Name, null), Times.Once);
//        }

//        [Fact]
//        public async Task MoveToBoard_ShouldMoveListCartAndAddActivities_WhenUserIsNotOwner()
//        {
//            var userId = Guid.NewGuid();
//            var cartListId = Guid.NewGuid();
//            var boardId = Guid.NewGuid();

//            var cartList = new ListCart
//            {
//                Id = cartListId,
//                UserId = Guid.NewGuid(),
//                Name = "Test List Cart",
//                BoardId = Guid.NewGuid(),
//                Board = new Board { Id = Guid.NewGuid(), Name = "Original Board" }
//            };

//            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(new ApplicationUser { Id = userId });

//            _listCartRepositoryMock.Setup(cl => cl.GetListCart(cartListId)).ReturnsAsync(cartList);

//            await Assert.ThrowsAsync<BadRequestException>(() => _listCartService.MoveToBoard(cartListId, boardId, userId));

//        }

//        [Fact]
//        public async Task GetLiastCartByBoardId_WhenBoardIsExsist()
//        {
//            var boardId = Guid.NewGuid();

//            var cartsList = new List<ListCart>
//    {
//        new ListCart
//        {
//            Id = Guid.NewGuid(),
//            UserId = Guid.NewGuid(),
//            Name = "Cart 1",
//            BoardId = boardId
//        },
//        new ListCart
//        {
//            Id = Guid.NewGuid(),
//            UserId = Guid.NewGuid(),
//            Name = "Cart 2",
//            BoardId = boardId
//        },
//        new ListCart
//        {
//            Id = Guid.NewGuid(),
//            UserId = Guid.NewGuid(),
//            Name = "Cart 3",
//            BoardId = boardId
//        }
//    };



//            _listCartRepositoryMock.Setup(cl => cl.GetLiastCartsByBoardId(boardId)).ReturnsAsync(cartsList);

//            var result = await _listCartService.GetLiastCartByBoardId(boardId);

//            Assert.NotNull(result);
//            Assert.Equal(3, result.Count);
//            Assert.All(result, r => Assert.Equal(boardId, r.BoardId));
//        }

//        [Fact]
//        public async Task GetLiastCartByBoardId_WhenCarttListIsNull()
//        {
//            var boardId = Guid.NewGuid();

//            _listCartRepositoryMock.Setup(cl => cl.GetLiastCartsByBoardId(boardId)).ReturnsAsync((List<ListCart>?)null);

//            await Assert.ThrowsAsync<NotFoundException>(() => _listCartService.GetLiastCartByBoardId(boardId));
//        }

//        [Fact]
//        public async Task GetLiastCartByBoardId_WhenCarttListIsEmpty()
//        {
//            var boardId = Guid.NewGuid();

//            _listCartRepositoryMock.Setup(cl => cl.GetLiastCartsByBoardId(boardId)).ReturnsAsync(new List<ListCart>());

//            var result = await _listCartService.GetLiastCartByBoardId(boardId);

//            Assert.NotNull(result);
//            Assert.Empty(result);
//        }
//    }
//}
