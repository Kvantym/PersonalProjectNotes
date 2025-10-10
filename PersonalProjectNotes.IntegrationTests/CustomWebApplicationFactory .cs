using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PersonalProjectNotes.Data;
using System;
using Microsoft.AspNetCore.Authorization;

namespace PersonalProjectNotes.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
           
            Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "true");

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }

    public class AllowAnonymousHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements)
            {
                context.Succeed(requirement); // автоматично успішно виконуємо всі вимоги
            }
            return Task.CompletedTask;
        }
    }
}
