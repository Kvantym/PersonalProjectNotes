using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalProjectNotes.Domain.Request.Cart;
using PersonalProjectNotes.Services;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        [Authorize]
        [HttpPost("create-cart")]
        public async Task<IActionResult> AddCart([FromBody] CreateCartRequest createCartRequest)
        {
            await _cartService.CreateCart(createCartRequest, User.GetUserId());
            return Ok(new { message = "Cart created successfully" });
        }
        [Authorize]
        [HttpPut("{cartId}")]
        public async Task<IActionResult> UpdateCart(Guid cartId, UpdateCartRequest updateCartRequest)
        {
            await _cartService.UpdateCart(cartId, updateCartRequest, User.GetUserId());
            return Ok(new { message = "Cart updated successfully" });
        }
        [Authorize]
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> RemoveCart(Guid cartId)
        {
            await _cartService.DeleteCart(cartId, User.GetUserId());
            return Ok(new { message = "Cart deleted successfully" });
        }
        [HttpGet("{cartId}")]
        public async Task<IActionResult> GetCart(Guid cartId)
        {
            var result = await _cartService.GetCart(cartId);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("carts-by-user")]
        public async Task<IActionResult> GetCarts()
        {
            var result =await _cartService.GetCarts(User.GetUserId());
            return Ok(result);
        }
        [Authorize]
        [HttpGet("move-to-cart-list{CartListId}")]
        public async Task<IActionResult> MoveToCartList(Guid cartId, Guid CartListId)
        {
           await _cartService.MoveToCardList(cartId, CartListId,User.GetUserId());
            return Ok(new { message = "Cart move to list successfully" });
        }
    }
}
