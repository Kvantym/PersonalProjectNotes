using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Domain.Request.ListCart;
using PersonalProjectNotes.Domain.Response;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class ListCartService : IListCartService
    {
        private readonly IListCartRepository _listCartRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartService _cartService;
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly IBoardRepository _boardRepository;

        public ListCartService(IListCartRepository listCartRepository, ICartRepository cartRepository, ICartService cartService, IUserService userService, IActivityService activityService, IBoardRepository boardRepository)
        {
            _listCartRepository = listCartRepository;
            _cartRepository = cartRepository;
            _cartService = cartService;
            _userService = userService; ;
            _activityService = activityService;
            _boardRepository = boardRepository;
        }

        public async Task CreateAsync(CreateListCartRequest listCartRequest, Guid userID, Guid boardId)
        {
            var cartList = new ListCart
            {
                UserId = userID,
                Name = listCartRequest.Name,
                BoardId = boardId,
                CreatedAt = DateTime.Now,
            };
            await _listCartRepository.Create(cartList);
            await _activityService.AddActivityToCartList(cartList.Id, UserAction.Create, userID);
            await _activityService.AddActivityToBoard(cartList.BoardId,UserAction.CreateListCart, userID, CreatelistCart:cartList);
        }

        public async Task DeleteAsync(Guid cartListId, Guid userId)
        {
            var listCart = await GetOrThrowListCart(cartListId);

            var cartsByList = await GetOrThrowGartsByList(cartListId);   
            if (listCart.UserId != userId)
            {
                throw new BadRequestException("You do not have permission to delete this list cart");
            }

            foreach (var cart in cartsByList)
            {
                await _cartRepository.Delete(cart);
            }

            await _activityService.AddActivityToBoard(listCart.BoardId,UserAction.DeleteListCart, userId,deleteListCart: listCart);

            await _listCartRepository.Delete(listCart);
        }

        public async Task<ListCartResponse> GetListCartAsync(Guid cartListId)
        {
            var listCart = await GetOrThrowListCart(cartListId);
            var carts = await _cartService.GetCarts(listCart.UserId);

            var response = CreateListCartResponse(listCart, carts);
            return response;
        }


        public async Task<List<ListCartResponse>> GetListCartsAsync(Guid userId)
        {
            var user = await _userService.GetOrThrowUser(userId);

            var listCarts = await GetThrowListsCartByUser(userId);

            var carts = await _cartService.GetCarts(userId);

            var response = listCarts.Select(listCart => CreateListCartResponse(listCart, carts)).ToList();
            return response;
        }


        public async Task UpdateAsync(Guid cartListId, UpdateListCartRequest listCartRequest, Guid userID)
        {
            var cartList = await GetOrThrowListCart(cartListId);

            var previousCartListState = new ListCart
            {
                Name = cartList.Name,               
                BoardId = cartList.BoardId,
            };
            if (cartList.UserId != userID)
            {
                throw new BadRequestException("You do not have permission to update this list cart");
            }

            cartList.Name = listCartRequest.Name;
            cartList.UpdatedAt = DateTime.Now;

            await _listCartRepository.Update(cartList);
            await _activityService.AddActivityToCartList(cartList.Id, UserAction.Update, userID, previousCartListState: previousCartListState);
        }

        public async Task MoveToBoard(Guid cartListId, Guid boardId, Guid userID)
        {
            var listCart = await GetOrThrowListCart(cartListId);
            var currentListCart = listCart;
            if (listCart.UserId != userID)
            {
                throw new BadRequestException("You do not have permission to move this list cart");
            }
            listCart.BoardId = boardId;
            var targetBoard = await _boardRepository.GetBoard(boardId);
            await _listCartRepository.Update(listCart);
            await _activityService.AddActivityToCartList(listCart.Id, UserAction.MoveToAtherBoard, userID, previousBoardName: currentListCart.Board.Name, targetBoardName: targetBoard.Name);
        }

        public async Task<List<ListCart>> GetThrowListsCartByUser(Guid userId)
        {
            var listCarts = await _listCartRepository.GetListCarts(userId);
            if (listCarts == null)
            {
                throw new NotFoundException("ListCarts not found");
            }

            if (listCarts == null || !listCarts.Any())
                return new List<ListCart>();
            return listCarts;
        }
        public async Task<ListCart> GetOrThrowListCart(Guid cartListId)
        {
            var listCart = await _listCartRepository.GetListCart(cartListId);
            if (listCart == null)
            {
                throw new NotFoundException("ListCart not found");
            }
            return listCart;
        }
        public async Task<List<Cart>> GetOrThrowGartsByList(Guid cartListId)
        {
            var cartsByList = await _listCartRepository.GetListCartsByListId(cartListId);
            if (cartsByList == null)
            {
                throw new NotFoundException("Carts not found");
            }
            return cartsByList;
        }

        public async Task<List<ListCart>> GetListCartByBoardId(Guid boardId, bool isArchive)
        {
            var cartsByList = await _listCartRepository.GetListCartsByBoardIdAndIsArchive(boardId, isArchive);
            if (cartsByList == null)
            {
                throw new NotFoundException("Carts not found");
            }
            return cartsByList;
        }

        public async Task<ListCart> GetListCartById(Guid cartListId)
        {
            var listCart = await GetOrThrowListCart(cartListId);
            return listCart;
        }

        public async Task<List<ActivityListCartResponse>> GetListCartActivityByListId(Guid cartListId)
        {
            var listCart = await GetOrThrowListCart(cartListId);
            return CreateActivityListCartResponse(listCart);
        }

        private List<ActivityListCartResponse> CreateActivityListCartResponse(ListCart listCart)
        {
            return listCart.ActivityListCarts?.Select(a => new ActivityListCartResponse
            {
                Action = a.Action,
                ActivityInformation = a.ActivityInformation,
                ActivityTime = a.ActivityTime,
            }).ToList();
        }

        private ListCartResponse CreateListCartResponse(ListCart listCart, List<CartResponse> carts)
        {
            return new ListCartResponse
            {
                ListCartId = listCart.Id,
                Name = listCart.Name,
                BoardId = listCart.BoardId,
                UserId = listCart.UserId,
                Carts = carts.Where(c => c.ListCartId == listCart.Id).ToList(),
                ActivityListCarts = CreateActivityListCartResponse(listCart)
            };
        }
        public async Task<bool> ListCartExists(Guid cartLisId)
        {
            var listCartExists = await _listCartRepository.ExistsAsync(cartLisId);
            if (!listCartExists)
                throw new InvalidOperationException($"ListCart with ID {cartLisId} does not exist.");
            return listCartExists;
        }

        public async Task UpdateCartListArchiveStatus(Guid userId, Guid listCartId, bool isArchive)
        {
            var listCart =  await GetOrThrowListCart(listCartId);
            if (listCart.UserId != userId)
            {
                throw new BadRequestException("You do not have permission to update this list cart");
            }
            await _listCartRepository.UpdateCartListArchiveStatus(listCart, isArchive);
        }

        public async Task<List<ListCart>> SearchListCartByName(Guid boardId, string cartName, bool isArchive)
        {
            if (cartName == null || cartName.Length == 0)
            {
                return await GetListCartByBoardId(boardId, isArchive);
            }

            return await _listCartRepository.SearchListCartByName(boardId, cartName, isArchive);
        }
    }
}
