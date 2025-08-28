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
        private readonly IUserRepository _userRepository;
        private readonly IListCartRepository _listCartRepository;

        public CartService(ICartRepository cartRepository, IUserRepository userRepository, IListCartRepository listCartRepository)
        {
            _cartRepository = cartRepository;
            _userRepository = userRepository;
            _listCartRepository = listCartRepository;
        }

        public async Task AddActivityToCart(Guid CartID, UserAction Action, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
               throw new NotFoundException($"User with ID {UserID} not found");
            }
            var cart = await _cartRepository.GetCart(CartID);
            if (cart == null)
            {
               throw new NotFoundException("Cart not found");
            }
            if (cart.UserId == UserID)
            {
                var Activity = new ActivityCart()
                {
                    Cart = cart,
                    CartId = cart.Id,
                    Action = Action,
                    UserId = UserID,
                    ActivityInformation = $"{user.UserName} виконав дію {Action} з карткою {cart.Name}",
                    ActivityTime = DateTime.UtcNow,
                };
                await _cartRepository.AddActivity(Activity);
            }
        }

        public async Task CreateCart(CreateCartRequest createCartRequest, Guid UserID)
        {
            var cart = new Cart()
            {
                UserId = UserID,
                Name = createCartRequest.Name,
                ListCartId = createCartRequest.ListCartId,
                DueDate = createCartRequest.DueDate,
                PriorityNote = createCartRequest.PriorityNote,
                StatusNote = createCartRequest.StatusNote,
                CreatedAt = DateTime.UtcNow

            };

            await _cartRepository.Create(cart);
            await AddActivityToCart(cart.Id, UserAction.Create, UserID);
        }

        public async Task DeleteCart(Guid cartID, Guid userID)
        {
            var cart = await _cartRepository.GetCart(cartID);

            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with ID {cartID} not found.");
            }

            await _cartRepository.Delete(cart);
            await AddActivityToCart(cartID, UserAction.Delete, userID);
        }


        public async Task<CartResponse> GetCart(Guid CartID)
        {
            var cart = await _cartRepository.GetCart(CartID);
            if (cart == null)
            {
                throw new NotFoundException("Cart not found");
            }
            var cartResponse = new CartResponse()
            {
                UserId = cart.UserId,
                Name = cart.Name,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                DueDate = cart.DueDate,
                ListCartId = cart.ListCartId,
                PriorityNote = cart.PriorityNote,
                StatusNote = cart.StatusNote,
                Action = cart.Action,
                ActivityCart = cart.ActivityCart.Select(a => new ActivityCartResponse
                {
                    Id = a.Id,
                    Action = a.Action,
                    ActivityInformation = a.ActivityInformation,
                    UserId = a.UserId,
                    ActivityTime = a.ActivityTime,
                    CartId = a.CartId,
                }).ToList()
            };
            return cartResponse;
        }

        public async Task<List<CartResponse>> GetCarts(Guid UserID)
        {
            var carts = await _cartRepository.GetCarts(UserID);
            if (carts == null)
            {
                throw new NotFoundException("Carts not found");
            }

            var cartList = carts.Select(c => new CartResponse
            {
                UserId = c.UserId,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DueDate = c.DueDate,
                ListCartId = c.ListCartId,
                PriorityNote = c.PriorityNote,
                StatusNote = c.StatusNote,
                Action = c.Action,

                ActivityCart = c.ActivityCart.Select(a => new ActivityCartResponse
                {
                    Id = a.Id,
                    Action = a.Action,
                    ActivityInformation = a.ActivityInformation,
                    UserId = a.UserId,
                    ActivityTime = a.ActivityTime,
                    CartId = a.CartId
                }).ToList()

            }).ToList();

            return cartList;
        }


        public async Task UpdateCart(Guid CartID, UpdateCartRequest updateCartRequest, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
                throw new NotFoundException($"User with ID {UserID} not found");

            var cart = await _cartRepository.GetCart(CartID);
            if (cart == null)
                throw new NotFoundException($"Cart with ID {CartID} not found");

            var listCartExists = await _listCartRepository.ExistsAsync(updateCartRequest.ListCartId);
            if (!listCartExists)
                throw new InvalidOperationException($"ListCart with ID {updateCartRequest.ListCartId} does not exist.");

            cart.Name = updateCartRequest.Name;
            cart.DueDate = updateCartRequest.DueDate;
            cart.ListCartId = updateCartRequest.ListCartId;
            cart.PriorityNote = updateCartRequest.PriorityNote;
            cart.StatusNote = updateCartRequest.StatusNote;
            cart.Action = UserAction.Update;
            cart.UpdatedAt = DateTime.UtcNow;


            await _cartRepository.Update(cart);
            await AddActivityToCart(CartID, UserAction.Update, UserID);
        }

        public async Task MoveToCardList(Guid CartID, Guid cartLisID, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
                throw new NotFoundException($"User with ID {UserID} not found");

            var cart = await _cartRepository.GetCart(CartID);
            if (cart == null)
                throw new NotFoundException($"Cart with ID {CartID} not found");

            var listCartExists = await _listCartRepository.ExistsAsync(cartLisID);
            if (!listCartExists)
                throw new InvalidOperationException($"ListCart with ID {cartLisID} does not exist.");

            cart.ListCartId = cartLisID;
            await _cartRepository.Update(cart);
            await AddActivityToCart(CartID, UserAction.MoveToAtherCartList, UserID);

        }

    }
}
