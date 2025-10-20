using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalProjectNotes.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PersonalProjectNotes.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "true");

            builder.ConfigureServices(services =>
            {
                    // Видаляємо існуючий DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Додаємо InMemory DbContext
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));

                    // 🧹 решта коду (автентифікація, авторизація) залишаємо
             
                // 🧹 Видаляємо будь-яку існуючу автентифікацію
                var authDescriptors = services
                    .Where(d => d.ServiceType == typeof(AuthenticationSchemeOptions))
                    .ToList();

                foreach (var d in authDescriptors)
                    services.Remove(d);

                // ⚙️ Реєструємо фейкову автентифікацію
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Fake";
                    options.DefaultChallengeScheme = "Fake";
                })
                .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("Fake", _ => { });

                // Дозволяємо всі політики авторизації
                services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                // Ініціалізуємо базу
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

            });


        }
    }

    public class AllowAnonymousHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements)
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FakeAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "11111111-1111-1111-1111-111111111111"), // ✅ Валідний GUID
            new Claim(ClaimTypes.Name, "TestUser"),
        };

            var identity = new ClaimsIdentity(claims, "Fake");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Fake");

            Context.User = principal; // 🔹 важливо: щоб User був доступний у контролері

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

}
