using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.Cart;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IListCartRepository _listCartRepository;
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly IListCartService _listCartService;


        public CartService(ICartRepository cartRepository, IListCartRepository listCartRepository, IUserService userService, IActivityService activityService, IListCartService listCartService)
        {
            _cartRepository = cartRepository;
            _listCartRepository = listCartRepository;
            _userService = userService;
            _activityService = activityService;
            _listCartService = listCartService;
        }

        public async Task CreateCart(CreateCartRequest createCartRequest, Guid userId, Guid cartListId)
        {
            if(cartListId == Guid.Empty)
            {
             throw new BadRequestException("CartListId cannot be null.");
            }
            var cart = new Cart()
            {
                Description = createCartRequest.Description,
                UserId = userId,
                Name = createCartRequest.Name,
                ListCartId = cartListId,
                DueDate = createCartRequest.DueDate,
                PriorityNote = createCartRequest.PriorityNote,
                StatusNote = createCartRequest.StatusNote,
                CreatedAt = DateTime.Now

            };
            await _cartRepository.Create(cart);
            await _activityService.AddActivityToCart(cart.Id, UserAction.Create, userId);
        }

        public async Task DeleteCart(Guid cartId, Guid userId)
        {
            var cart = await GetOrThrowCart(cartId);
            if (cart.UserId != userId)
            {
                throw new BadRequestException("You do not have permission to delete this cart");
            }
            await _activityService.AddActivityToCartList(cart.ListCartId,UserAction.DeleteCartWithCartList, userId,deletedCart: cart);
            await _cartRepository.Delete(cart);
        }

        public async Task<CartResponse> GetCart(Guid cartId)
        {
            var cart = await GetOrThrowCart(cartId);
            var cartResponse = CreateCartResponse(cart);
            return cartResponse;
        }

        public async Task<List<CartResponse>> GetCarts(Guid userId)
        {
            var carts = await GetOrThrowCartsByUserId(userId);
            var cartList = carts.Select(cart => CreateCartResponse(cart)).ToList();
            return cartList;
        }

        public async Task UpdateCart(Guid cartId, UpdateCartRequest updateCartRequest, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            var cart = await GetOrThrowCart(cartId);

            var listCartExists = await _listCartService.ListCartExists(updateCartRequest.ListCartId);
            if (cart.UserId != userId)
            {
                throw new BadRequestException("You do not have permission to move this cart");
            }

            var previousCartState = new Cart
            {
                Name = cart.Name,
                Description = cart.Description,
                PriorityNote = cart.PriorityNote,
                StatusNote = cart.StatusNote,
                DueDate = cart.DueDate,
                ListCartId = cart.ListCartId
            };

            cart.Description = updateCartRequest.Description;
            cart.Name = updateCartRequest.Name;
            cart.DueDate = updateCartRequest.DueDate;
            cart.ListCartId = updateCartRequest.ListCartId;
            cart.PriorityNote = updateCartRequest.PriorityNote;
            cart.StatusNote = updateCartRequest.StatusNote;
            cart.Action = UserAction.Update;
            cart.UpdatedAt = DateTime.Now;

            await _cartRepository.Update(cart);
            await _activityService.AddActivityToCart(cartId, UserAction.Update, userId, previousCartState);
        }


        public async Task MoveToCardList(Guid cartId, Guid cartLisId, Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);
            var cart = await GetOrThrowCart(cartId);
            var targetList = await _listCartRepository.GetListCart(cartLisId); 
            var previousList = await _listCartRepository.GetListCart(cart.ListCartId); 
            if (cart.UserId != userId)
            {
                throw new BadRequestException("You do not have permission to move this cart");
            }

            var previousCartState = new Cart
            {
                ListCartId = cart.ListCartId
            };

            cart.ListCartId = cartLisId;
            cart.Action = UserAction.MoveToAtherCartList;
            cart.UpdatedAt = DateTime.Now;

            await _cartRepository.Update(cart);
            await _activityService.AddActivityToCart(cartId, UserAction.MoveToAtherCartList, userId, previousCartState, previousList?.Name, targetList?.Name);
        }

        public async Task<bool> ListCartExists(Guid cartLisId)
        {
            var listCartExists = await _listCartRepository.ExistsAsync(cartLisId);
            if (!listCartExists)
                throw new InvalidOperationException($"ListCart with ID {cartLisId} does not exist.");
            return listCartExists;
        }

        public async Task<List<Cart>> GetCartsByListCart(Guid ListCartId)
        {
            var carts = await _cartRepository.GetCartsByCartList(ListCartId);
            if (carts == null || !carts.Any())
            {
                return new List<Cart>();
            }
            return carts;
        }

        public async Task<Cart> GetOrThrowCart(Guid cartId)
        {
            var cart = await _cartRepository.GetCart(cartId);
            if (cart == null)
            {
                throw new NotFoundException($"Cart with ID {cartId} not found");
            }
            return cart;
        }
        public async Task<List<Cart>> GetOrThrowCartsByUserId(Guid userId)
        {
            var carts = await _cartRepository.GetCarts(userId);
            if (carts == null || !carts.Any())
            {
                return new List<Cart>();
            }
            return carts;
        }
        private List<ActivityCartResponse> CreateActivityCartResponse(Cart cart)
        {
            return cart.ActivityCart.Select(a => new ActivityCartResponse
            {
                Id = a.Id,
                Action = a.Action,
                ActivityInformation = a.ActivityInformation,
                UserId = a.UserId,
                ActivityTime = a.ActivityTime,
                CartId = a.CartId
            }).ToList();
        }
        private CartResponse CreateCartResponse(Cart cart)
        {
            return new CartResponse
            {
                Description = cart.Description,
                UserId = cart.UserId,
                Name = cart.Name,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                DueDate = cart.DueDate,
                ListCartId = cart.ListCartId,
                PriorityNote = cart.PriorityNote,
                StatusNote = cart.StatusNote,
                Action = cart.Action,
                ActivityCart = CreateActivityCartResponse(cart)
            };
        }
        public async Task<List<ActivityCartResponse>> GetActivityCart(Guid cartId)
        {
            var cart = await GetOrThrowCart(cartId);
            var activityCart = await _cartRepository.GetActivityCart(cartId);
            if(activityCart == null)
            {
                throw new NotFoundException($"CartActivity not found");
            }

            return activityCart.Select(ac => new ActivityCartResponse
            {
                Id = ac.Id,
                Action = ac.Action,
                ActivityInformation = ac.ActivityInformation,
                UserId = ac.UserId,
                ActivityTime = ac.ActivityTime,
                CartId = ac.CartId
            }).ToList();
        }
    }
}
//263