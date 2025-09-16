using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalProjectNotes.Domain.Request.Account;
using PersonalProjectNotes.Services;
using PersonalProjectNotes.Services.Interfaces;

namespace PersonalProjectNotes.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;


        public AccountController(IAccountService accountService, IUserService userService)
        {
            _accountService = accountService;
            _userService = userService;
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

        [Authorize]
        [HttpPut("update-user-name")]
        public async Task UpdateUserName([FromForm] string newUserName)
        { 
            await _userService.UpdateUserName(User.GetUserId(), newUserName);
        }
        [Authorize]
        [HttpPut("update-user-password")]
        public async Task UpdateUserPassword([FromForm] string newUserPassword)
        {
            await _userService.UpdateUserPassword(User.GetUserId(), newUserPassword);
        }
        [Authorize]
        [HttpPut("update-user-email")]
        public async Task<IActionResult> UpdateUserEmail([FromForm] string newUserEmail)
        {
            if(string.IsNullOrEmpty(newUserEmail)|| !newUserEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Електронна пошта має закінчуватися на @gmail.com");
            }
            await _userService.UpdateUserEmail(User.GetUserId(), newUserEmail);
            return Ok(new { message = "Email updated successfully" });
        }

    }
}
