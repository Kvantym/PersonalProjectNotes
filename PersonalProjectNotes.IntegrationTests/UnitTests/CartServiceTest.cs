using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Cart;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Repositories.Repositories;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;

namespace PersonalProjectNotes.IntegrationTests.UnitTests
{
    public class CartServiceTest
    {
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<IListCartRepository> _listCartRepositorMocky;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IActivityService> _activityServiceMock;
        private readonly Mock<IListCartService> _listCartService;
        private readonly Mock<IListCartService> _listCartServiceMock;

        private readonly ICartService _cartService;

        public CartServiceTest()
        {
            _cartRepositoryMock = new Mock<ICartRepository>();
            _listCartRepositorMocky = new Mock<IListCartRepository>();
            _userServiceMock = new Mock<IUserService>();
            _activityServiceMock = new Mock<IActivityService>();
            _listCartServiceMock = new Mock<IListCartService>();

            _cartService = new Services.Services.CartService(_cartRepositoryMock.Object, _listCartRepositorMocky.Object, _userServiceMock.Object, _activityServiceMock.Object);
        }

        [Fact]
        public async Task CreateCartTest_AndAddActivity()
        {
            var createCartRequest = new CreateCartRequest
            {
                Name = "Test Cart",
                Description = "This is a test cart",
                DueDate = DateTime.UtcNow.AddDays(7),
            };
            var userId = Guid.NewGuid();
            var cartListId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId };
            var listCart = new ListCart { Id = cartListId, UserId = userId, Name = "Test List Cart" };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(user);
            _listCartRepositorMocky.Setup(l => l.GetListCart(cartListId)).ReturnsAsync(listCart);
            _cartRepositoryMock.Setup(c => c.Create(It.IsAny<Cart>())).Returns(Task.CompletedTask);
            _activityServiceMock.Setup(a => a.AddActivityToCart(It.IsAny<Guid>(), UserAction.Create, userId, null, null, null));

            await _cartService.CreateCart(createCartRequest, userId, cartListId);

            _cartRepositoryMock.Verify(c => c.Create(It.IsAny<Cart>()), Times.Once);
            _activityServiceMock.Verify(a => a.AddActivityToCart(It.IsAny<Guid>(), UserAction.Create, userId, null, null, null), Times.Once);

        }

        [Fact]
        public async Task CreateCart_WhenCartListIdIsNull()
        {
            var createCartRequest = new CreateCartRequest
            {
                Name = "Test Cart",
                Description = "This is a test cart",
                DueDate = DateTime.UtcNow.AddDays(7),
            };
            var userId = Guid.NewGuid();
            var cartListId = Guid.Empty;

            await Assert.ThrowsAsync<BadRequestException>(()=> _cartService.CreateCart(createCartRequest,userId,cartListId));
        }

        [Fact]
        public async Task DeleteCart_WhenUserIsOwner()
        {
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cart = new Cart
            {
                Id = cartId,
                UserId =userId,
                Name = "Test Cart",
                ListCartId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            var user = new ApplicationUser { Id = userId };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(user);
            _cartRepositoryMock.Setup(c => c.GetCart(cartId)).ReturnsAsync(cart);
            _activityServiceMock.Setup(a => a.AddActivityToCartList(cart.ListCartId, UserAction.DeleteCartWithCartList, userId, null,null,null,cart));

            await _cartService.DeleteCart(cartId, userId);

   
            _cartRepositoryMock.Verify(c => c.Delete(cart), Times.Once);
            _activityServiceMock.Verify(a => a.AddActivityToCartList(cart.ListCartId, UserAction.DeleteCartWithCartList, userId, null, null, null, cart), Times.Once);
        }    

        [Fact]
        public async Task DeleteCart_WhenUserIsNotOwner()
        {
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cart = new Cart
            {
                Id = cartId,
                UserId = Guid.NewGuid(), 
                Name = "Test Cart",
                ListCartId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            var user = new ApplicationUser { Id = userId };

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(user);
            _cartRepositoryMock.Setup(c => c.GetCart(cartId)).ReturnsAsync(cart);
            await Assert.ThrowsAsync<BadRequestException>(()=> _cartService.DeleteCart(cartId, userId));
        }

        [Fact]
        public async Task GetCart_ByCartIdTest()
        {
            var cartId = Guid.NewGuid();

            var cart = new Cart
            {
                Id = cartId,
                Name = "Test Cart",
                ListCartId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _cartRepositoryMock.Setup(s => s.GetCart(cartId)).ReturnsAsync(cart);

            var result = await _cartService.GetCart(cartId);

            Assert.NotNull(result);
            Assert.Equal(cart.Name, result.Name);
            _cartRepositoryMock.Verify(r => r.GetCart(cartId),Times.Once);
        }

        [Fact]
        public async Task GetCarts_ByUserIdTest()
        {
            var cartId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var carts = new List<Cart>()
{
    new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ListCartId = Guid.NewGuid(),
        Name = "Cart 1",
        CreatedAt = DateTime.UtcNow
    },
    new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ListCartId = Guid.NewGuid(),
        Name = "Cart 2",
        CreatedAt = DateTime.UtcNow
    }
};


            _cartRepositoryMock.Setup(c => c.GetCarts(userId)).ReturnsAsync(carts);

            var result = await _cartService.GetCarts(userId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            _cartRepositoryMock.Verify(c => c.GetCarts(userId), Times.Once);
        }
        //[Fact]
        //public async Task UpdateCart_WhenUserIsOwner_And_CartIdIsValid_AddActivity()
        //{
        //    var userId = Guid.NewGuid();
        //    var cartId = Guid.NewGuid();
        //    var updateCartRequest = new UpdateCartRequest
        //    {
        //        Name = "Updated Cart",
        //        Description = "Updated Description",
        //        DueDate = DateTime.UtcNow.AddDays(5),
        //        ListCartId = Guid.NewGuid(),
        //    };

        //    var user = new ApplicationUser { Id = userId };
        //    var cart = new Cart
        //    {
        //        Id = cartId,
        //        UserId = userId,
        //        Name = "Original Cart",
        //        Description = "Original Description",
        //        DueDate = DateTime.UtcNow.AddDays(2),
        //        ListCartId = Guid.NewGuid(),
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    _userServiceMock.Setup(u => u.GetOrThrowUser(userId))
        //        .ReturnsAsync(user);

        //    _cartRepositoryMock.Setup(c => c.GetCart(cartId))
        //        .ReturnsAsync(cart);

        //    _listCartServiceMock.Setup(l => l.ListCartExists(updateCartRequest.ListCartId))
        //        .ReturnsAsync(true);

        //    _cartRepositoryMock.Setup(c => c.Update(It.IsAny<Cart>()))
        //        .Returns(Task.CompletedTask);

        //    _activityServiceMock.Setup(a => a.AddActivityToCart(
        //        cartId,
        //        UserAction.Update,
        //        userId,
        //        It.IsAny<Cart>(),
        //        null,
        //        null))
        //        .Returns(Task.CompletedTask);

        //    // Act
        //    await _cartService.UpdateCart(cartId, updateCartRequest, userId);

        //    // Assert
        //    Assert.Equal(updateCartRequest.Name, cart.Name);
        //    Assert.Equal(updateCartRequest.Description, cart.Description);
        //    Assert.Equal(updateCartRequest.ListCartId, cart.ListCartId);

        //    _cartRepositoryMock.Verify(c => c.Update(It.IsAny<Cart>()), Times.Once);
        //    _activityServiceMock.Verify(a => a.AddActivityToCart(
        //        cartId,
        //        UserAction.Update,
        //        userId,
        //        It.IsAny<Cart>(),
        //        null,
        //        null), Times.Once);
        //}




        [Fact]
        public async Task UpdateCart_WhenCartIdIsNotValid()
        {
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var updateCartRequest = new UpdateCartRequest
            {
                Name = "Updated Cart",
                Description = "Updated Description",
                DueDate = DateTime.UtcNow.AddDays(5),
                ListCartId = Guid.NewGuid(),
            };

            var user = new ApplicationUser { Id = userId };

            Cart? cart = null;

            _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(user);
            _cartRepositoryMock.Setup(c => c.GetCart(cartId)).ReturnsAsync(cart);
            await Assert.ThrowsAsync<NotFoundException>(() => _cartService.UpdateCart(cartId, updateCartRequest, userId));
            _cartRepositoryMock.Verify(c => c.Update(It.IsAny<Cart>()), Times.Never);
        }

        //[Fact]
        //public async Task UpdateCart_WhenUserIsNotOwner()
        //{
        //    var userId = Guid.NewGuid();
        //    var cartId = Guid.NewGuid();
        //    var updateCartRequest = new UpdateCartRequest
        //    {
        //        Name = "Updated Cart",
        //        Description = "Updated Description",
        //        DueDate = DateTime.UtcNow.AddDays(5),
        //        ListCartId = Guid.NewGuid(),
        //    };
        //    var cart = new Cart
        //    {
        //        Id = cartId,
        //        UserId = Guid.NewGuid(),
        //        Name = "Original Cart",
        //        Description = "Original Description",
        //        DueDate = DateTime.UtcNow.AddDays(2),
        //        ListCartId = Guid.NewGuid(),
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    var user = new ApplicationUser { Id = userId };
        //    _userServiceMock.Setup(u => u.GetOrThrowUser(userId)).ReturnsAsync(user);
        //    _cartRepositoryMock.Setup(c => c.GetCart(cartId)).ReturnsAsync(cart);
        //    await Assert.ThrowsAsync<BadRequestException>(() => _cartService.UpdateCart(cartId, updateCartRequest, userId));
        //}

        [Fact]
        public async Task MoveToCardList_Test()
        {
            var cartId = Guid.NewGuid();
            var cartLisId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser { Id = userId };
            var cart = new Cart { Id = cartId, UserId = userId, ListCartId = Guid.NewGuid() };
            var targetList = new ListCart { Id = cartLisId, UserId = userId , Name = "TargetName" };
            var previousList = new ListCart { Id = cart.ListCartId, Name="PreviousName" };

            var previousCartState = new Cart {Id = cart.ListCartId};


            _userServiceMock.Setup(u=> u.GetOrThrowUser(userId)).ReturnsAsync(user);
            _cartRepositoryMock.Setup(c=>c.GetCart(cartId)).ReturnsAsync(cart);
            _listCartRepositorMocky.Setup(c => c.GetListCart(cartLisId)).ReturnsAsync(targetList);
            _listCartRepositorMocky.Setup(c => c.GetListCart(cart.ListCartId)).ReturnsAsync(previousList);

            _cartRepositoryMock.Setup(c => c.Update(cart));

            _activityServiceMock.Setup(a => a.AddActivityToCart(cartId, UserAction.MoveToAtherCartList, userId, It.IsAny<Cart>(), previousList.Name,targetList.Name));

            await _cartService.MoveToCardList(cartId,cartLisId,userId);

            Assert.Equal(targetList.Id, cart.ListCartId);
           // Assert.Equal(UserAction.MoveToAtherCartList, cart.Action);

            _cartRepositoryMock.Verify(c=>c.Update(It.IsAny<Cart>()),Times.Once);
            _activityServiceMock.Verify(a=>a.AddActivityToCart(cartId,UserAction.MoveToAtherCartList, userId, It.IsAny<Cart>(), previousList.Name,targetList.Name));

        }

        [Fact]
        public async Task MoveToCardList_WhenUserIsNotOwner()
        {
            var cartId = Guid.NewGuid();
            var cartLisId = Guid.NewGuid();
            var ownerId = Guid.NewGuid(); 
            var otherUserId = Guid.NewGuid();

            var user = new ApplicationUser { Id = otherUserId };
            var cart = new Cart { Id = cartId, UserId = ownerId, ListCartId = Guid.NewGuid() };
            var targetList = new ListCart { Id = cartLisId, UserId = ownerId };
            var previousList = new ListCart { Id = cart.ListCartId };

            var previousCartState = new Cart { Id = cart.ListCartId };

            _userServiceMock.Setup(u=> u.GetOrThrowUser(otherUserId)).ReturnsAsync(user);
            _cartRepositoryMock.Setup(c => c.GetCart(cartId)).ReturnsAsync(cart);
            _listCartRepositorMocky.Setup(c => c.GetListCart(cartLisId)).ReturnsAsync(targetList);
            _listCartRepositorMocky.Setup(c => c.GetListCart(cart.ListCartId)).ReturnsAsync(previousList);

            await  Assert.ThrowsAsync<BadRequestException>(() => _cartService.MoveToCardList(cartId, cartLisId, otherUserId));

        }

        [Fact]
        public async Task GetCartsByListCart_Test()
        {
            var cartListId = Guid.NewGuid();
            var carts = new List<Cart>
{
    new Cart
    {
        Id = Guid.NewGuid(),
        ListCartId = cartListId,
        Name = "Cart 1",
        CreatedAt = DateTime.Now
    },
    new Cart
    {
        Id = Guid.NewGuid(),
        ListCartId = cartListId,
        Name = "Cart 2",
        CreatedAt = DateTime.Now
    }
};

            _cartRepositoryMock.Setup(c=>c.GetCartsByCartList(cartListId)).ReturnsAsync(carts);

            var result = await _cartService.GetCartsByListCart(cartListId);

            Assert.NotNull(result);
            Assert.Equal(2,result.Count);

        }

        [Fact]
        public async Task GetCartsByListCart_WhenListCartIsNotExsist()
        {
            var cartListId = Guid.NewGuid();

            _cartRepositoryMock.Setup(c => c.GetCartsByCartList(cartListId)).ReturnsAsync((List<Cart>?)null);

            var result = await _cartService.GetCartsByListCart(cartListId);

            Assert.NotNull(result);
            Assert.Empty(result);

        }

        [Fact]
        public async Task GetOrThrowCart_Test()
        {
            var cartId = Guid.NewGuid();
            var cart = new Cart { Id = cartId };

            _cartRepositoryMock.Setup(c=> c.GetCart(cartId)).ReturnsAsync(cart);

          var result = await _cartService.GetOrThrowCart(cartId);
           Assert.NotNull(result);
        }

        [Fact]
        public async Task GetOrThrowCart_WhenCartIsNotExsist()
        {
            var cartId = Guid.NewGuid();
            var cart = new Cart { Id = cartId };

            _cartRepositoryMock.Setup(c => c.GetCart(cartId)).ReturnsAsync((Cart?)null);

             await Assert.ThrowsAsync<NotFoundException>(()=> _cartService.GetOrThrowCart(cartId));

            _cartRepositoryMock.Verify(c=> c.GetCart(cartId),Times.Once);
           
        }

        [Fact]
        public async Task GetOrThrowCartsByUserId_TestIfCartsIsEmpty()
        {
            var userId = Guid.NewGuid();

            var carts = new List<Cart>();

            _cartRepositoryMock.Setup(c => c.GetCarts(userId)).ReturnsAsync(carts);

            var result = await _cartService.GetOrThrowCartsByUserId(userId);

            Assert.Empty(result);
        }
        [Fact]
        public async Task GetOrThrowCartsByUserId_TestIfCartsIsNotEmpty()
        {
            var userId = Guid.NewGuid();

            var carts = new List<Cart>
{
    new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ListCartId = Guid.NewGuid(),
        Name = "Cart 1",
        CreatedAt = DateTime.Now
    },
    new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ListCartId = Guid.NewGuid(),
        Name = "Cart 2",
        CreatedAt = DateTime.Now
    }
};

            _cartRepositoryMock.Setup(c => c.GetCarts(userId)).ReturnsAsync(carts);

            var result = await _cartService.GetOrThrowCartsByUserId(userId);

            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal(userId, c.UserId));
        }

       
    }
}
 
