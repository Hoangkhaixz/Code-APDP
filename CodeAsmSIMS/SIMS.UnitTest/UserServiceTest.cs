using Xunit;
using Moq;
using SIMSS.Services;
using SIMSS.Interfaces;
using SIMSS.SimsDbContext.Entities;
using System.Threading.Tasks;

namespace SIMSS.UnitTests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task LoginUserAsync_ValidPlainPassword_ReturnsUser()
        {
            // Arrange: User có password dạng plain text (không nên dùng thực tế)
            var user = new Users
            {
                Username = "admin",
                PasswordHash = "1234"
            };

            // Mock repository → trả về user
            var repoMock = new Mock<IUserRepository>();
            repoMock.Setup(r => r.GetUserByUsernameAsync("admin"))
                    .ReturnsAsync(user);

            // Mock password hasher → kiểm tra plain text: Verify(hash, inputPassword)
            var hasherMock = new Mock<IPasswordHasher>();
            hasherMock.Setup(h => h.Verify("1234", "1234"))
                      .Returns(true);

            // Inject cả repo và hasher
            var service = new UserService(repoMock.Object, hasherMock.Object);

            // Act
            var result = await service.LoginUserAsync("admin", "1234");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("admin", result.Username);
        }
        [Fact]
        public async Task LoginUserAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Trường hợp user không tồn tại trong database
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(x => x.GetUserByUsernameAsync("notfound")).ReturnsAsync((Users)null);
            var mockHasher = new Mock<IPasswordHasher>();
            var service = new UserService(mockRepo.Object, mockHasher.Object);

            var result = await service.LoginUserAsync("notfound", "abc");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsNull_WhenPasswordIsIncorrect()
        {
            // Trường hợp user tồn tại nhưng password sai
            var mockRepo = new Mock<IUserRepository>();
            var user = new Users { Username = "testuser", PasswordHash = "hashedpassword" };
            mockRepo.Setup(x => x.GetUserByUsernameAsync("testuser")).ReturnsAsync(user);

            var mockHasher = new Mock<IPasswordHasher>();
            mockHasher.Setup(x => x.Verify("wrongpassword", user.PasswordHash)).Returns(false);

            var service = new UserService(mockRepo.Object, mockHasher.Object);

            var result = await service.LoginUserAsync("testuser", "wrongpassword");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsNull_WhenUsernameIsNullOrEmpty()
        {
            // Trường hợp username null hoặc empty, không tìm kiếm database
            var mockRepo = new Mock<IUserRepository>();
            var mockHasher = new Mock<IPasswordHasher>();
            var service = new UserService(mockRepo.Object, mockHasher.Object);

            var result1 = await service.LoginUserAsync(null, "abc");
            var result2 = await service.LoginUserAsync("", "abc");

            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsNull_WhenPasswordIsNullOrEmpty()
        {
            // Trường hợp password null hoặc empty, user tồn tại nhưng không thể verify
            var mockRepo = new Mock<IUserRepository>();
            var user = new Users { Username = "testuser", PasswordHash = "hashedpassword" };
            mockRepo.Setup(x => x.GetUserByUsernameAsync("testuser")).ReturnsAsync(user);

            var mockHasher = new Mock<IPasswordHasher>();
            var service = new UserService(mockRepo.Object, mockHasher.Object);

            var result1 = await service.LoginUserAsync("testuser", null);
            var result2 = await service.LoginUserAsync("testuser", "");

            Assert.Null(result1);
        }
    }
}


