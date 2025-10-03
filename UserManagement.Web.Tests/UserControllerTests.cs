using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }


    [Fact]
    public void List_WhenFilteringActiveUsers_ShouldReturnOnlyActive()
    {
        // Arrange
        var controller = CreateController();
        var activeUser = SetupUsers(forename: "Alice", isActive: true).First();
        var inactiveUser = SetupUsers(forename: "Bob", isActive: false).First();
        _userService.Setup(s => s.GetAll()).Returns([activeUser, inactiveUser]);

        // Act
        var result = controller.List(isActive: true);

        // Assert
        var model = result.Model as UserListViewModel;
        model.Should().NotBeNull();
        model!.Items.Should().ContainSingle(u => u.Forename == "Alice");
        model.Items.Should().NotContain(u => u.Forename == "Bob");
    }

    [Fact]
    public void List_WhenFilteringInactiveUsers_ShouldReturnOnlyInactive()
    {
        // Arrange
        var controller = CreateController();
        var activeUser = SetupUsers(forename: "Charlie", isActive: true).First();
        var inactiveUser = SetupUsers(forename: "Dana", isActive: false).First();
        _userService.Setup(s => s.GetAll()).Returns([activeUser, inactiveUser]);

        // Act
        var result = controller.List(isActive: false);

        // Assert
        var model = result.Model as UserListViewModel;
        model.Should().NotBeNull();
        model!.Items.Should().ContainSingle(u => u.Forename == "Dana");
        model.Items.Should().NotContain(u => u.Forename == "Charlie");
    }

    [Fact]
    public void List_WhenFilterResultsInNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var controller = CreateController();
        var activeUser = SetupUsers(isActive: true).First();
        _userService.Setup(s => s.GetAll()).Returns(new[] { activeUser });

        // Act
        var result = controller.List(isActive: false);

        // Assert
        var model = result.Model as UserListViewModel;
        model.Should().NotBeNull();
        model!.Items.Should().BeEmpty();
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
