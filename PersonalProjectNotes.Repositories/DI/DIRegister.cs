using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalProjectNotes.Repositories.Interfaces;
using PersonalProjectNotes.Repositories.Repositories;

namespace PersonalProjectNotes.Repositories.DI
{
    public static class DIRegister
    {
        public static IServiceCollection ConfigureRepositoriesDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBoardRepository, BoardRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IListCartRepository, ListCartRepository>();

            return services;
        }
    }
}
