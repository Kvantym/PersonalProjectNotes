using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using PersonalProjectNotes.Domain.Entities;
using PersonalProjectNotes.Domain.Request.Account;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PersonalProjectNotes.IntegrationTests.UnitTests
{
    public class AccountServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly IConfiguration _configuration;

        public AccountServiceTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Єдиний конфіг для всіх тестів
            var inMemorySettings = new Dictionary<string, string>
{
    {"JwtSettings:SecretKey", "THIS_IS_A_SUPER_SECRET_KEY_1234567890123456"},
    {"JwtSettings:Issuer", "TestIssuer"},
    {"JwtSettings:Audience", "TestAudience"}
};


            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task LoginAsync_ReturnsToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                NormalizedUserName = "TESTUSER",
                Id = Guid.NewGuid()
            };

            var request = new LoginRequest { Username = "testuser", Password = "Password123!" };

            _userManagerMock.Setup(x => x.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                              .ReturnsAsync(SignInResult.Success);

            var service = new AccountService(
                null, _configuration, _userManagerMock.Object,
                _signInManagerMock.Object, _httpContextAccessorMock.Object);

            // Act
            var token = await service.LoginAsync(request);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_Error_WhenIUserNotExists()
        {
            var service = new AccountService(
                null, _configuration, _userManagerMock.Object,
                _signInManagerMock.Object, _httpContextAccessorMock.Object);

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            _userManagerMock.Setup(m => m.FindByNameAsync(request.Username))
                .ReturnsAsync((ApplicationUser)null);

            var exception = await Assert.ThrowsAsync<AuthorizationException>(() => service.LoginAsync(request));

            // Assert
            exception.Message.Should().Contain("Невірне ім'я користувача.");
        }

        [Fact]
        public async Task LoginAsync_Error_WhenInvalidPassword()
        {
            var service = new AccountService(
                null, _configuration, _userManagerMock.Object,
                _signInManagerMock.Object, _httpContextAccessorMock.Object);

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                UserName = "testuser",
                NormalizedUserName = "TESTUSER",
            };

            _userManagerMock.Setup(m => m.Users)
      .Returns(new List<ApplicationUser> { user }.AsQueryable());

            _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), request.Password, false)).ReturnsAsync(SignInResult.Failed);
            var exception = await Assert.ThrowsAsync<AuthorizationException>(() => service.LoginAsync(request));
            exception.Message.Should().Contain("Невірний пароль.");
        }

        [Fact]
        public async Task LoginAsync_Error_WhenInvalidUserName()
        {
            var service = new AccountService(
                null, _configuration, _userManagerMock.Object,
                _signInManagerMock.Object, _httpContextAccessorMock.Object);

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            _userManagerMock.Setup(m => m.FindByNameAsync(request.Username)).ReturnsAsync((ApplicationUser)null);


            var exception = await Assert.ThrowsAsync<AuthorizationException>(() => service.LoginAsync(request));
            exception.Message.Should().Contain("Невірне ім'я користувача.");

        }


        [Fact]
        public async Task RegisterUserAsync_ReturnsToken_WhenSuccessful()
        {
            // Arrange
            var service = new AccountService(
                null, _configuration, _userManagerMock.Object,
                _signInManagerMock.Object, _httpContextAccessorMock.Object);

            var request = new RegisterUserRequest
            {
                Username = "testuser",
                Email = "test@test.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(m => m.FindByNameAsync(request.Username))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var token = await service.RegisterUserAsync(request);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RegisterUserAsync_WhenUserExists()
        {
           
            var service = new AccountService(
                null, _configuration, _userManagerMock.Object,
                _signInManagerMock.Object, _httpContextAccessorMock.Object);

            var request = new RegisterUserRequest
            {
                Username = "testuser",
                Email = "test@test.com",
                Password = "Password123!"
            };


            _userManagerMock.Setup(m => m.FindByNameAsync(request.Username))
                .ReturnsAsync(new ApplicationUser { UserName = request.Username});

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterUserAsync(request));
        }

        [Fact]
        public async Task RegisterUserAsync_WhenCreateFails_ThrowsInvalidOperationException()
        {
            var service = new AccountService(null, _configuration, _userManagerMock.Object, _signInManagerMock.Object, _httpContextAccessorMock.Object);

            var request = new RegisterUserRequest
            {
                Username = "testuser",
                Email = "test@test.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(m=> m.FindByNameAsync(request.Username)).ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(m=>m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterUserAsync(request));

            exception.Message.Should().Contain("Створення користувача не вдалося");
            exception.Message.Should().Contain("Error");
        }

    }
}
