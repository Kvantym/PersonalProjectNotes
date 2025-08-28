using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalProjectNotes.Domain.Request.ListCart;
using PersonalProjectNotes.Services;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/list-cart")]
    public class ListCartController: ControllerBase
    {
        private readonly IListCartService _listCartService;

        public ListCartController(IListCartService listCartService)
        {
            _listCartService = listCartService;
        }
        [Authorize]
        [HttpPost("create-list-cart/{boardId}")]
        public async Task<IActionResult> AddListCart([FromBody] CreateListCartRequest createListCartRequest, Guid boardId)
        {
           await _listCartService.CreateAsync(createListCartRequest, User.GetUserId(), boardId);
            return Ok(new { message = "CartList created successfully" });
        }
        [Authorize]
        [HttpPut("{listCartId}")]
        public async Task<IActionResult> UpdateListCart([FromBody] UpdateListCartRequest updateListCartRequest, Guid listCartId)
        {
            await _listCartService.UpdateAsync(listCartId, updateListCartRequest, User.GetUserId());
            return Ok(new { message = "CartList updated successfully" });
        }
        [Authorize]
        [HttpDelete("{listCartId}")]
        public async Task<IActionResult> RemoveListCart(Guid listCartId)
        {
           await _listCartService.DeleteAsync(listCartId, User.GetUserId());
           return Ok(new { message = "CartList deleted successfully" });
        }
        [HttpGet("list-cart-by-id/{listCartId}")]
        public async Task<IActionResult> GetListCart(Guid listCartId)
        {
            return Ok( await _listCartService.GetListCartAsync(listCartId));
        }
        [Authorize]
        [HttpGet("list-cart-by-user")]
        public async Task<IActionResult> GetListCarts()
        {
           return Ok(await _listCartService.GetListCartsAsync(User.GetUserId()) );
        }

        [Authorize]
        [HttpGet("move-to-board{boardId}")]
        public async Task<IActionResult> MoveToBoard(Guid boardId, Guid cartListId)
        {
           await _listCartService.MoveToBoard(cartListId, boardId, User.GetUserId());
           return Ok(new { message = "CartList move to board successfully" });
        }
    }
}
