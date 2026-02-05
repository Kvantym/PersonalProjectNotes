using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Enums;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Services.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IUserService _userService;
        private readonly ICartRepository _cartRepository;
        private readonly IListCartRepository _listCartRepository;
        private readonly IBoardRepository _boardRepository;

        public ActivityService(IUserService userService, ICartRepository cartRepository, IListCartRepository listCartRepository, IBoardRepository boardRepository)
        {
            _userService = userService;
            _cartRepository = cartRepository;
            _listCartRepository = listCartRepository;
            _boardRepository = boardRepository;
        }

        public async Task AddActivityToCart(Guid cartId, UserAction userAction, Guid userId, Cart? previousCartState = null, string? previousListName = null, string? targetListName = null)
        {
            var user = await _userService.GetOrThrowUser(userId);
            var cart = await _cartRepository.GetCart(cartId);

            string activityInfo = userAction switch
            {
                UserAction.Create => $"{user.UserName} створив картку '{cart.Name}'",
                UserAction.Delete => $"{user.UserName} видалив картку '{cart.Name}'",
                UserAction.MoveToAtherCartList => $"{user.UserName} перемістив картку '{cart.Name}' з '{previousListName}' до '{targetListName}'",
                UserAction.Update when previousCartState != null => GenerateUpdateCartActivity(user.UserName, previousCartState, cart),
                _ => $"{user.UserName} виконав дію {userAction} з карткою '{cart.Name}'"
            };

            if (string.IsNullOrEmpty(activityInfo))
                return;

            var activity = new ActivityCart
            {
                CartId = cart.Id,
                Action = userAction,
                UserId = userId,
                ActivityInformation = activityInfo,
                ActivityTime = DateTime.Now
            };

            await _cartRepository.AddActivity(activity);
        }

        public async Task AddActivityToCartList(Guid cartListId, UserAction userAction, Guid userId, ListCart? previousCartListState = null, string? previousBoardName = null, string? targetBoardName = null, Cart? deletedCart = null)
        {
            var user = await _userService.GetOrThrowUser(userId);
            var cartList = await _listCartRepository.GetListCart(cartListId);

            string activityInfo = userAction switch
            {
                UserAction.Create => $"{user.UserName} створив список '{cartList.Name}'",
                UserAction.Delete => $"{user.UserName} видалив список '{cartList.Name}'",
                UserAction.DeleteCartWithCartList =>  $"{user.UserName} видалив картку '{deletedCart.Name}' зі списку '{cartList.Name}'",
                UserAction.MoveToAtherBoard => $"{user.UserName} перемістив список '{cartList.Name}' з '{previousBoardName}' до '{targetBoardName}'",
           
                UserAction.Update when previousCartListState != null => GenerateUpdateCartListActivity(user.UserName, previousCartListState, cartList),
                _ => $"{user.UserName} виконав дію {userAction} зі списком '{cartList.Name}'"
            };

            if (string.IsNullOrEmpty(activityInfo))
                return;

            var activity = new ActivityListCart
            {
                ListCartId = cartListId,
                Action = userAction,
                UserId = userId,
                ActivityInformation = activityInfo,
                ActivityTime = DateTime.Now
            };

            await _listCartRepository.AddActivity(activity);
        }

        public async Task AddActivityToBoard(Guid boardId, UserAction userAction, Guid userId, Board? previousBoardState = null, ListCart? deleteListCart = null ,ListCart? CreatelistCart= null, ApplicationUser? coloboration = null)
        {
            var user = await _userService.GetOrThrowUser(userId);
            var board = await _boardRepository.GetBoard(boardId);

            string activityInfo = userAction switch
            {
                UserAction.Create => $"{user.UserName} створив дошку '{board.Name}'",
                UserAction.Delete => $"{user.UserName} видалив дошку '{board.Name}'",
                UserAction.CreateListCart => $"{user.UserName} створив список '{CreatelistCart?.Name}' на дошці '{board.Name}'",
                UserAction.DeleteListCart => $"{user.UserName} видалив список '{deleteListCart?.Name}' з дошки '{board.Name}'",
                UserAction.Update when previousBoardState != null => GenerateUpdateBoardActivity(user.UserName, previousBoardState, board),
                UserAction.RemoveUserIfYouOwner => $"{user.UserName} видалив користувача '{coloboration.UserName}'",
                UserAction.LeaveUser => $"Користувач {user.UserName} покинив дошку",
                UserAction.AddUser => $"{user.UserName} додав користувача '{coloboration.UserName}'",
                _ => $"{user.UserName} виконав дію {userAction} з дошкою '{board.Name}'",
                
            };

            if (string.IsNullOrEmpty(activityInfo))
                return;

            var activity = new ActivityBoard
            {
                BoardId = boardId,
                Action = userAction,
                UserId = userId,
                ActivityInformation = activityInfo,
                ActivityTime = DateTime.Now
            };

            await _boardRepository.AddActivity(activity);
        }

        private string GenerateUpdateCartActivity(string userName, Cart previous, Cart current)
        {
            List<string> changes = new List<string>();

            if (previous.Name != current.Name)
                changes.Add($"Назва: '{previous.Name}' → '{current.Name}'");
            if (previous.Description != current.Description)
                changes.Add($"Опис: '{previous.Description}' → '{current.Description}'");
            if (previous.PriorityNote != current.PriorityNote)
                changes.Add($"Пріоритет: '{previous.PriorityNote}' → '{current.PriorityNote}'");
            if (previous.StatusNote != current.StatusNote)
                changes.Add($"Статус: '{previous.StatusNote}' → '{current.StatusNote}'");
            if (previous.DueDate != current.DueDate)
                changes.Add($"Дата виконання: '{previous.DueDate}' → '{current.DueDate}'");
            if (previous.ListCartId != current.ListCartId)
                changes.Add($"Список: '{previous.ListCartId}' → '{current.ListCartId}'");

            if (!changes.Any())
                return string.Empty;

            return $"{userName} оновив картку '{current.Name}': {string.Join(", ", changes)}";
        }

        private string GenerateUpdateCartListActivity(string userName, ListCart previous, ListCart current, Cart? deletedCart = null)
        {
            List<string> changes = new List<string>();

            if (previous.Name != current.Name)
                changes.Add($"Назва: '{previous.Name}' → '{current.Name}'");
            if (previous.BoardId != current.BoardId)
                changes.Add($"Опис: '{previous.Board.Name}' → '{current.Board.Name}'");
            if(deletedCart!= null)
                changes.Add($"Було видалено карту{deletedCart.Name} з дошки {previous.Name}");


            if (!changes.Any())
                return string.Empty;

            return $"{userName} оновив спиоск листів '{current.Name}': {string.Join(", ", changes)}";
        }

        private string GenerateUpdateBoardActivity(string userName, Board previous, Board current, ListCart? deletedCarList= null)
        {
            List<string> changes = new List<string>();

            if (previous.Name != current.Name)
                changes.Add($"Назва: '{previous.Name}' → '{current.Name}'");
            if (deletedCarList!= null)
                changes.Add($"Було видалено список карт{deletedCarList.Name} з дошки {previous.Name}");

            if (!changes.Any())
                return string.Empty;

            return $"{userName} оновив дошку. Зміни: " + string.Join(", ", changes);
        }
    }
}
