using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.ListCart;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Repositories.Repositories;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class ListCartService : IListCartService
    {
        private readonly IListCartRepository _listCartRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IUserRepository _userRepository;

        public ListCartService(IListCartRepository listCartRepository, ICartRepository cartRepository, IUserRepository userRepository)
        {
            _listCartRepository = listCartRepository;
            _cartRepository = cartRepository;
            _userRepository = userRepository;
        }

        public async Task AddActivityToCartList(Guid CartListID, UserAction Action, Guid UserID)
        {
            var user = await _userRepository.GetByIdAsync(UserID);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {UserID} not found");
            }
            var cartList = await _listCartRepository.GetListCart(CartListID);
            if (cartList == null)
            {
                throw new NotFoundException("Cart not found");
            }
            if (cartList.UserId == UserID)
            {
                var Activity = new ActivityListCart()
                {
                    ListCartId = CartListID, 
                    Action = Action,
                    UserId = UserID,
                    ActivityInformation = $"{user.UserName} виконав дію {Action} з карткою {cartList.Name}",
                    ActivityTime = DateTime.UtcNow,
                };
                await _listCartRepository.AddActivity(Activity);
            }
        }

        public async Task CreateAsync(CreateListCartRequest listCart, Guid userID, Guid boardId)
        {
            var cartList = new ListCart
            {
                UserId = userID,
                Name = listCart.Name,
                BoardId = boardId,
                CreatedAt = DateTime.UtcNow,
            };
            await _listCartRepository.Create(cartList);
            await AddActivityToCartList(cartList.Id, UserAction.Create, userID);
        }

        public async Task DeleteAsync(Guid listCartId, Guid userID)
        {
            var cartList = await _listCartRepository.GetListCart(listCartId);
            if (cartList == null)
            {
                throw new NotFoundException("ListCart not found");
            }
            var carts = await _listCartRepository.GetListCartsByListId(listCartId);
            if (carts == null)
            {
                throw new NotFoundException("Carts not found");
            }
            foreach (var cart in carts)
            {
                await _cartRepository.Delete(cart);
            }
            await _listCartRepository.Delete(cartList);
            await AddActivityToCartList(listCartId, UserAction.Delete, userID);
        }

        public async Task<ListCartResponse> GetListCartAsync(Guid listCartId)
        {
            var listCart = await _listCartRepository.GetListCart(listCartId);
            if (listCart == null) 
            {
            throw new NotFoundException("ListCart not Found");
            }

            if (listCart == null)
            {
                throw new NotFoundException("ListCart not Found");
            }

            var response = new ListCartResponse
            {
                Name = listCart.Name,
                BoardId = listCart.BoardId,
                UserId = listCart.UserId,

                Carts = listCart.Carts.Select(c => new CartResponse
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

                    ActivityCart = c.ActivityCart?.Select(a => new ActivityCartResponse
                    {
                        Id = a.Id,
                        Action = a.Action,
                        ActivityInformation = a.ActivityInformation,
                        UserId = a.UserId,
                        ActivityTime = a.ActivityTime,
                        CartId = a.CartId,
                    }).ToList()
                }).ToList(),

                ActivityListCarts = listCart.ActivityListCarts?.Select(a => new ActivityListCartResponse
                {
                    Id = a.Id,
                    Action = a.Action,
                    ActivityInformation = a.ActivityInformation,
                    UserId = a.UserId,
                    ActivityTime = a.ActivityTime,
                    ListCartId = a.ListCartId
                }).ToList()
            };


            return response;
        }


        public async Task<List<ListCartResponse>> GetListCartsAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }

            var listCarts = await _listCartRepository.GetListCarts(userId);
            if(listCarts== null)
            {
                throw new NotFoundException("ListCarts not found");
            }

            if (listCarts == null || !listCarts.Any())
                throw new NotFoundException("ListCarts not found");

            var response = listCarts.Select(listCart => new ListCartResponse
            {
                Name = listCart.Name,
                BoardId = listCart.BoardId,
                UserId = listCart.UserId,

                Carts = listCart.Carts.Select(c => new CartResponse
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

                    ActivityCart = c.ActivityCart.Select(a => new ActivityCartResponse
                    {
                        Id = a.Id,
                        Action = a.Action,
                        ActivityInformation = a.ActivityInformation,
                        UserId = a.UserId,
                        ActivityTime = a.ActivityTime,
                        CartId = a.CartId
                    }).ToList()


                }).ToList(),
                     ActivityListCarts = listCart.ActivityListCarts?.Select(a => new ActivityListCartResponse
                     {
                         Id = a.Id,
                         Action = a.Action,
                         ActivityInformation = a.ActivityInformation,
                         UserId = a.UserId,
                         ActivityTime = a.ActivityTime,
                         ListCartId = a.ListCartId
                     }).ToList()

            }).ToList();

            return response;
        }


        public async Task UpdateAsync(Guid cartLisrId, UpdateListCartRequest listCart, Guid userID)
        {
            var cartList = await _listCartRepository.GetListCart(cartLisrId);

            if (cartList == null)
            {
               throw new NotFoundException("ListCart not found");
            }

            cartList.Name = listCart.Name;
            cartList.BoardId = listCart.BoardId;
            cartList.UpdatedAt = DateTime.UtcNow;

            await _listCartRepository.Update(cartList);
            await AddActivityToCartList(cartList.Id, UserAction.Update, userID);
        }

        public async Task MoveToBoard(Guid listCartId, Guid boardId, Guid userID)
        {
            var listCart = await _listCartRepository.GetListCart(listCartId);
            if (listCart == null)
            {
                throw new NotFoundException("ListCart not found");
            }
            listCart.BoardId = boardId;
            await _listCartRepository.Update(listCart);
            await AddActivityToCartList(listCart.Id, UserAction.MoveToAtherBoard, userID);
        }
    }

   
}
