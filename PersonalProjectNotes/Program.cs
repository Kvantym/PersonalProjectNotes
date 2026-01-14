using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

            // 1. Конфігурація CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy => policy
                        .WithOrigins("http://localhost:4200", "https://witty-pebble-0fc40b00f.1.azurestaticapps.net")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()); // Дозволяє передачу токенів/кукі, якщо потрібно
            });

            // 2. Базові сервіси
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddEndpointsApiExplorer();

            // 3. Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Personal Project Notes API", Version = "v1" });
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
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 4. База даних (MySQL або InMemory)
            var useInMemory = Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true";
            if (useInMemory)
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Connection string 'DefaultConnection' is missing!");
                }
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }

            // 5. Identity
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

            // 6. JWT Authentication
            var jwtSection = configuration.GetSection("JwtSettings");
            builder.Services.Configure<JwtSettings>(jwtSection);
            var jwtSettings = jwtSection.Get<JwtSettings>();

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
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? "VerySecretDefaultKey")),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            // 7. DI для Репозиторіїв та Сервісів
            builder.Services.ConfigureRepositoriesDI(configuration);
            builder.Services.ConfigureServices(configuration);

            var app = builder.Build();

            // --- КОНВЕЄР ОБРОБКИ (MIDDLEWARE) ---

            // CORS МАЄ БУТИ ПЕРШИМ (до автентифікації та ендпоінтів)
            app.UseCors("AllowAngular");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                });
            }
            else 
            {
                // В Azure Swagger зазвичай теж потрібен для тестів, 
                // тому можна винести його з-під if IsDevelopment
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting(); // Додаємо явно для чіткості

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Автоматична міграція (обережно в продакшені)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // db.Database.Migrate(); 
            }

            app.Run();
        }
    }
}
// Для Integration Tests
public partial class Program { }
