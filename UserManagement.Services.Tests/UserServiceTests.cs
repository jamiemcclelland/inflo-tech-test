using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeSameAs(users);
    }

    [Fact]
    public void FilterByActive_WhenTrue_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
            new User { Forename = "ActiveUser", IsActive = true },
            new User { Forename = "InactiveUser", IsActive = false }
        }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.FilterByActive(true).ToList();

        // Assert
        result.Should().ContainSingle()
              .Which.Forename.Should().Be("ActiveUser");
    }

    [Fact]
    public void FilterByActive_WhenFalse_ReturnsOnlyInactiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
            new User { Forename = "ActiveUser", IsActive = true },
            new User { Forename = "InactiveUser", IsActive = false }
        }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.FilterByActive(false).ToList();

        // Assert
        result.Should().ContainSingle()
              .Which.Forename.Should().Be("InactiveUser");
    }

    private IQueryable<User> SetupUsers(string forename = "Johnny", string surname = "User", string dateOfBirth = "01/01/2000", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                DateOfBirth = dateOfBirth,
                Email = email,
                IsActive = isActive
            }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
