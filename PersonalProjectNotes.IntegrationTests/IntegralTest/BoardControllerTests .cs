using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Request.Board;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PersonalProjectNotes.IntegrationTests.IntegralTest
{
    public class BoardControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BoardControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateBoard_ReturnsOk_WhenValidRequest()
        {
            var facory = new CustomWebApplicationFactory();
            var client = facory.CreateClient();

            var boardRequest = new CreateBoardRequest
            {
                Name = "Test Board",
            };

            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // такий же, як у FakeAuth

            using (var scope = facory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                var user = new ApplicationUser
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "test@example.com"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            var response = await client.PostAsJsonAsync("/api/board/create-board", boardRequest);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseMessage>();
            Assert.Equal("Board created successfully", result.Message);
           
        }

        [Fact]
        public async Task DeleeBoardTest()
        {
            var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            var boardId = Guid.NewGuid();
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // такий же, як у FakeAuth

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                var user = new ApplicationUser
                {
                    Id = userId,
                    UserName = "testuser",

                };
                var board = new Board
                {
                    Id = boardId,
                    Name = "Board to Delete",
                    UserId = userId
                };
                context.Users.Add(user);
                context.Boards.Add(board);
                await context.SaveChangesAsync();
            }
            var response = await client.DeleteAsync($"/api/board/{boardId}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseMessage>();
            Assert.Equal("Board deleted successfully", result.Message);
        }

        [Fact]
        public async Task GetBoardById()
        {
            var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            var boardId = Guid.NewGuid();
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // такий же, як у FakeAuth

            using (var scope = factory.Services.CreateScope())
            {
                               var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                var user = new ApplicationUser
                {
                    Id = userId,
                    UserName = "testuser",
                };
                var board = new Board
                {
                    Id = boardId,
                    Name = "Board to Get",
                    UserId = userId
                };
                context.Users.Add(user);
                context.Boards.Add(board);
                await context.SaveChangesAsync();
            }

            var response = await client.GetAsync($"/api/board/board-by-id/{boardId}");
            response.EnsureSuccessStatusCode();
            var boardResult = await response.Content.ReadFromJsonAsync<Board>();
            Assert.Equal("Board to Get", boardResult.Name);
            Assert.Equal(userId, boardResult.UserId);

        }

        [Fact]
        public async Task UpdateBoard_WithInMemoryDb_ReturnsOk()
        {
            var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();

            var boardId = Guid.NewGuid();
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // такий же, як у FakeAuth

            // Додаємо користувача і борд
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var user = new ApplicationUser
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "test@example.com"
                };
                context.Users.Add(user);

                var board = new Board
                {
                    Id = boardId,
                    Name = "Initial Board",
                    UserId = userId
                };
                context.Boards.Add(board);

                await context.SaveChangesAsync();
            }

            // PUT-запит
            var request = new UpdateBoardRequest { Name = "Updated Board" };
            var response = await client.PutAsJsonAsync($"/api/board/{boardId}", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseMessage>();
            Assert.Equal("Board update successfully", result.Message);

            // Перевірка оновлення
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var board = await context.Boards.FindAsync(boardId);
                Assert.Equal("Updated Board", board.Name);
                Assert.Equal(userId, board.UserId);
            }
        }





    }


    public class FakePolicyEvaluator : IPolicyEvaluator
    {
        public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            var principal = new System.Security.Claims.ClaimsPrincipal();
            principal.AddIdentity(new System.Security.Claims.ClaimsIdentity(new[]
            {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier,
                                              "11111111-1111-1111-1111-111111111111")
        }, "Test"));

            context.User = principal;

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(principal, "Test")));
        }

        public Task<PolicyAuthorizationResult> AuthorizeAsync(
            AuthorizationPolicy policy,
            AuthenticateResult authenticationResult,
            HttpContext context,
            object resource)
        {
            return Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }


    public class ResponseMessage
    {
        public string Message { get; set; }
    }
}
