using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Services;
using PersonalProjectNotes.Services.Interfaces;
using System.IO.Compression;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/board")]
    public class BoardController : ControllerBase
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
            return Ok(await _boardService.GetBoards(User.GetUserId(), false));
        }
        [Authorize]
        [HttpGet("get-activity-board/{boardId}")]
        public async Task<IActionResult> GetBoardsByUser(Guid boardId)
        {
            var result = await _boardService.GetActivityBoardById(boardId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("add-collaborator/{boardId}")]
        public async Task<IActionResult> AddCollaborator(Guid boardId, [FromQuery] string identifier)
        {

            await _boardService.AddColloborator(boardId, identifier, User.GetUserId());
            return Ok(new { message = $"Collaborator {identifier} added successfully" });
        }
        [Authorize]
        [HttpGet("get-collaborators/{boardId}")]
        public async Task<IActionResult> GetAllCollaborators(Guid boardId)
        {
            var result = await _boardService.GetAllColloborators(boardId);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("delete-collaborator/{boardId}/{collaborationName}")]
        public async Task<IActionResult> DeleteCollaborator(Guid boardId, string collaborationName)
        {
            await _boardService.DeleteUserFromBoardIfUserIsOwner(boardId, User.GetUserId(), collaborationName);
            return Ok(new { message = "Collaborator deleted successfully" });
        }

        [Authorize]
        [HttpDelete("remove-collaborator/{boardId}")]
        public async Task<IActionResult> RemoveCollaborator(Guid boardId)
        {
            await _boardService.RemoveColoboratorFromBoard(boardId, User.GetUserId());
            return Ok(new { message = "Collaborator removed successfully" });
        }


        [Authorize]
        [HttpPost("archive-board/{boardId}")]
        public async Task<IActionResult> ArchiveBoard(Guid boardId)
        {
            await _boardService.ToggleArchiveStatus(User.GetUserId(), boardId, true);
            return Ok(new { message = "Board archived successfully" });
        }

        [Authorize]
        [HttpPost("unarchive-board/{boardId}")]
        public async Task<IActionResult> UnarchiveBoard(Guid boardId)
        {
            await _boardService.ToggleArchiveStatus(User.GetUserId(), boardId, false);
            return Ok(new { message = "Board unarchived successfully" });
        }
        [Authorize]
        [HttpGet("board-by-user-if-isArchive-true")]
        public async Task<IActionResult> GetBoardsByUserIfIsArchive()
        {
            return Ok(await _boardService.GetBoards(User.GetUserId(), true));
        }

        [HttpGet("get-search-board")]
        public async Task<IActionResult> GetSearchBoard([FromQuery] string? searchName, bool isArchive)
        {
            var result = await _boardService.SearchBoardsByName(User.GetUserId(), searchName, isArchive);
            return Ok(result);
        }

    }
}
