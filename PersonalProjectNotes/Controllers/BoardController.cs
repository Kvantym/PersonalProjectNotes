using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Services;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/board")]
    public class BoardController: ControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }
       [Authorize]
        [HttpPost("create-board")]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest createBoardRequest)
        {
                await _boardService.CreateBoard(createBoardRequest, User.GetUserId());
                return Ok(new { message = "Board created successfully" });        
        }
        [Authorize]
        [HttpPut("{boardId}")]
        public async Task<IActionResult> UpdateBoard([FromBody] UpdateBoardRequest updateBoardRequest, Guid boardId)
        {
            await _boardService.UpdateBoard(boardId, updateBoardRequest, User.GetUserId());
            return Ok(new { message = "Board update successfully" });
        }
        [Authorize]
        [HttpDelete("{boardId}")]
        public async Task<IActionResult> DeleteBoard(Guid boardId)
        {
            await _boardService.DeleteBoard(boardId, User.GetUserId());
            return Ok(new { message = "Board deleted successfully" });
        }
        [Authorize]
        [HttpGet("board-by-id/{boardId}")]
        public async Task<IActionResult> GetBoardById(Guid boardId)
        {
            return Ok(await _boardService.GetBoard(boardId, User.GetUserId()));
        }

        [Authorize]
        [HttpGet("board-by-user")] 
        public async Task<IActionResult> GetBoardsByUser()
        {
            return Ok(await _boardService.GetBoards(User.GetUserId()));
        }
        [Authorize]
        [HttpGet("get-activity-board/{boardId}")]
        public async Task<IActionResult> GetBoardsByUser(Guid boardId)
        {
            var result = await _boardService.GetActivityBoardById(boardId);
            return Ok(result);
        }


    }
}
