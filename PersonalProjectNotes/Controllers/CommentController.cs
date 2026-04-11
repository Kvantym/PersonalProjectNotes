using Microsoft.AspNetCore.Mvc;
using PersonalProjectNotes.Domain.Request.Comment;
using PersonalProjectNotes.Services;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentServices _comentService;
        public CommentController(ICommentServices comentService)
        {
            _comentService = comentService;
        }

        [HttpPost("add-comment/{cartId}")]
        public async Task<IActionResult> AddCommentToCart(Guid cartId, [FromBody] AddCommentResquest addCommentResquest)
        {
            await _comentService.AddCommentToCart(cartId, User.GetUserId(), addCommentResquest.Content);
            return Ok(new { message = "Comment added successfully" });
        }

        [HttpGet("comments-by-cart/{cartId}")]
        public async Task<IActionResult> GetCommentsByCartId(Guid cartId)
        {
            var comments = await _comentService.GetCommentsByCartId(cartId, User.GetUserId());
            return Ok(comments);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            await _comentService.DeleteComment(commentId, User.GetUserId());
            return Ok(new { message = "Comment deleted successfully" });
        }
    }
}
