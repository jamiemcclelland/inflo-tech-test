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

    [Fact]
    public void Add_WhenUserHasNoId_AssignsNextIdAndCallsCreate()
    {
        // Arrange
        var service = CreateService();
        var existingUsers = new[]
        {
            new User { Id = 1, Forename = "ExistingUser", IsActive = true }
        }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(existingUsers);

        var newUser = new User { Forename = "NewUser", IsActive = true, Id = 0 };

        // Act
        service.Add(newUser);

        // Assert
        newUser.Id.Should().Be(2);
        _dataContext.Verify(d => d.Create(newUser), Times.Once);
    }

    [Fact]
    public void Add_WhenNoExistingUsers_AssignsIdOneAndCallsCreate()
    {
        // Arrange
        var service = CreateService();
        var emptyUsers = Enumerable.Empty<User>().AsQueryable();
        _dataContext.Setup(s => s.GetAll<User>()).Returns(emptyUsers);

        var newUser = new User { Forename = "FirstUser", IsActive = true, Id = 0 };

        // Act
        service.Add(newUser);

        // Assert
        newUser.Id.Should().Be(1);
        _dataContext.Verify(d => d.Create(newUser), Times.Once);
    }

    [Fact]
    public void Add_WhenUserHasExistingId_UsesProvidedIdAndCallsCreate()
    {
        // Arrange
        var service = CreateService();
        var existingUsers = new[]
        {
            new User { Id = 1, Forename = "ExistingUser", IsActive = true }
        }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(existingUsers);

        var newUser = new User { Id = 99, Forename = "PreAssignedIdUser", IsActive = true };

        // Act
        service.Add(newUser);

        // Assert
        newUser.Id.Should().Be(99);
        _dataContext.Verify(d => d.Create(newUser), Times.Once);
    }

    [Fact]
    public void Delete_WhenUserExists_ShouldCallDeleteOnce()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Id = 1, Forename = "Test", Surname = "User" };
        var users = new[] { user }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        service.Delete(1);

        // Assert
        _dataContext.Verify(s => s.Delete(user), Times.Once);
    }

    [Fact]
    public void Delete_WhenUserDoesNotExist_ShouldNotCallDelete()
    {
        // Arrange
        var service = CreateService();
        var users = new[] { new User { Id = 1, Forename = "Test", Surname = "User" } }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        service.Delete(2);

        // Assert
        _dataContext.Verify(s => s.Delete(It.IsAny<User>()), Times.Never);
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
