using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalProjectNotes.Services.Interfaces;
using PersonalProjectNotes.Services.Services;

namespace PersonalProjectNotes.Services.DI
{
    public static class DIRegister
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IBoardService, BoardService>();
            services.AddScoped<IListCartService, ListCartService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<ICommentServices, CommentServices>();
            return services;
        }
    }
}
