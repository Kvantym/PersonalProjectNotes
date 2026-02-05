using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyShopProjectBackend.Services;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Repositories.DI;
using PersonalProjectNotes.Services.DI;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace PersonalProjectNotes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // 1. Налаштування CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy => policy
                    .WithOrigins("http://localhost:4200", "http://localhost:53126", "https://witty-pebble-0fc40b00f.1.azurestaticapps.net")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddEndpointsApiExplorer();

            // 2. Налаштування Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My Shop Project API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

          // 3. База даних
var connectionString = configuration.GetConnectionString("DefaultConnection");
var useInMemory = Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true";

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useInMemory || string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("TestDb");
    }
    else
    {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            //  options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),mySqlOptions => mySqlOptions.MigrationsAssembly("PersonalProjectNotes.Data"));
        }
});

            // 4. Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 5. JWT з перевіркою на NULL (головна причина помилки 500.30)
            var jwtSection = configuration.GetSection("JwtSettings");
            var jwtSettings = jwtSection.Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(jwtSection);

            // Використовуємо значення або дефолтні заглушки, щоб програма запустилася
            var secretKey = jwtSettings?.SecretKey ?? "A_Very_Long_Emergency_Secret_Key_123456789";
            var issuer = jwtSettings?.Issuer ?? "PersonalProjectNotes";
            var audience = jwtSettings?.Audience ?? "PersonalProjectNotesUser";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            builder.Services.ConfigureRepositoriesDI(configuration);
            builder.Services.ConfigureServices(configuration);

            var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Це автоматично створить таблиці в Azure MySQL, якщо їх там немає
        if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Помилка під час застосування міграцій бази даних.");
    }
}

            app.UseRouting();
            // --- ПОРЯДОК MIDDLEWARE ---
            app.UseCors("AllowAngular");

            if (app.Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
app.UseDeveloperExceptionPage();
            app.Run();
        }
    }
}
