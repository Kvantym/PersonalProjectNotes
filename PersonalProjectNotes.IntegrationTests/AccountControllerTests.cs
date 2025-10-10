using FluentAssertions; // Для більш зручних та читабельних assertion-ів у тестах
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing; // Для WebApplicationFactory, яка дозволяє створювати тестовий сервер
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // Для доступу до DI контейнера, щоб підміняти сервіси
using Moq; // Для створення моків (імітацій) сервісів
using PersonalProjectNotes; // Простір імен твого проекту
using PersonalProjectNotes.Controllers;
using PersonalProjectNotes.Data;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Request.Account; // Моделі запитів LoginRequest та RegisterUserRequest
using PersonalProjectNotes.Services.Interfaces; // Інтерфейс IAccountService
using PersonalProjectNotes.Services.Services;
using System.Net; // Для HttpStatusCode
using System.Net.Http.Headers;
using System.Net.Http.Json; // Для методів PostAsJsonAsync та ReadFromJsonAsync
using System.Security.Claims;
using System.Text.Json; // Для JsonElement та десеріалізації JSON
using System.Threading.Tasks; // Для async/await
using Xunit; // Для атрибутів тестів [Fact] та запуску тестів

namespace PersonalProjectNotes.IntegrationTests
{
    // Клас тестів для AccountController
    public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client; // HTTP-клієнт для відправки запитів до тестового сервера

        // Конструктор тестового класу, отримує фабрику тестового сервера
        public AccountControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(); // Створюємо HttpClient для роботи з сервером
        }



       
    }
}
