using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalProjectNotes.Domain.Request.Account;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;


        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Domain.Request.Account.LoginRequest loginRequest)
        {
            var result = await _accountService.LoginAsync(loginRequest);
            return Ok(new {token = result});
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerRequest)
        {
            var resulr = await _accountService.RegisterUserAsync(registerRequest);
            return Ok(resulr);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok(new { message = "Користувач вийшов із системи" });
        }

        [Authorize]
        [HttpGet("current-user")]
        public async Task<IActionResult> GetGurrentUser()
        {
            var result = await _accountService.GetCurrentUserAsync();
            return Ok(result);
        }

    }
}
