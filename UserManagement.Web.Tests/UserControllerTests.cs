using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        _userService.Setup(s => s.GetAll()).Returns(users);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserPageViewModel>()
            .Which.Users.Should().BeEquivalentTo(users, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public void Create_Post_ValidModel_CallsAddAndRedirects()
    {
        // Arrange
        var controller = CreateController();
        var newUserVm = new UserCreateViewModel
        {
            Forename = "Alice",
            Surname = "Smith",
            DateOfBirth = "2000-01-26",
            Email = "alice@example.com",
            IsActive = true
        };

        var model = new UserPageViewModel
        {
            User = newUserVm
        };

        // Act
        var result = controller.Create(model);

        // Assert
        _userService.Verify(s => s.Add(It.Is<User>(u =>
            u.Forename == "Alice" &&
            u.Surname == "Smith" &&
            u.DateOfBirth == "26/01/2000" &&
            u.Email == "alice@example.com" &&
            u.IsActive == true
        )), Times.Once);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
    }

    [Fact]
    public void Create_Post_InvalidModel_ReturnsListViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Forename", "Required");

        var newUserVm = new UserCreateViewModel
        {
            Forename = "",
            Surname = "Smith",
            DateOfBirth = "2000-01-26",
            Email = "alice@example.com",
            IsActive = true
        };

        var model = new UserPageViewModel
        {
            User = newUserVm
        };

        var existingUsers = SetupUsers();
        _userService.Setup(s => s.GetAll()).Returns(existingUsers);

        // Act
        var result = controller.Create(model);

        // Assert
        _userService.Verify(s => s.Add(It.IsAny<User>()), Times.Never);

        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("List");

        var returnedModel = viewResult.Model as UserPageViewModel;
        returnedModel.Should().NotBeNull();
        returnedModel.User.Should().BeEquivalentTo(newUserVm);
        returnedModel.Users.Should().BeEquivalentTo(existingUsers, opts => opts.ExcludingMissingMembers());
    }

        [Fact]
    public void Edit_Post_ValidModel_CallsUpdateAndRedirects()
    {
        // Arrange
        var controller = CreateController();
        var model = new UserPageViewModel
        {
            User = new UserCreateViewModel
            {
                Forename = "John",
                Surname = "Doe",
                DateOfBirth = "1999-12-31",
                Email = "john@example.com",
                IsActive = true
            }
        };

        // Act
        var result = controller.Edit(1, model);

        // Assert
        _userService.Verify(s => s.Update(1, It.Is<User>(u =>
            u.Forename == "John" &&
            u.Surname == "Doe" &&
            u.DateOfBirth == "31/12/1999" &&
            u.Email == "john@example.com" &&
            u.IsActive == true
        )), Times.Once);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
    }

    [Fact]
    public void Edit_Post_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Forename", "Required");

        var model = new UserPageViewModel
        {
            User = new UserCreateViewModel
            {
                Forename = "",
                Surname = "Doe",
                DateOfBirth = "1999-12-31",
                Email = "john@example.com",
                IsActive = true
            }
        };

        // Act
        var result = controller.Edit(1, model);

        // Assert
        _userService.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<User>()), Times.Never);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid user data.");
    }

    [Fact]
    public void Edit_Post_ReformatsDateBeforeCallingUpdate()
    {
        // Arrange
        var controller = CreateController();
        var model = new UserPageViewModel
        {
            User = new UserCreateViewModel
            {
                Forename = "Jane",
                Surname = "Smith",
                DateOfBirth = "2001-02-15",
                Email = "jane@example.com",
                IsActive = false
            }
        };

        User? capturedUser = null;
        _userService.Setup(s => s.Update(It.IsAny<int>(), It.IsAny<User>()))
            .Callback<int, User>((id, user) => capturedUser = user);

        // Act
        controller.Edit(42, model);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.DateOfBirth.Should().Be("15/02/2001");
    }

    [Fact]
    public void Delete_WhenCalled_ShouldCallServiceDeleteOnce()
    {
        // Arrange
        var controller = CreateController();
        var userId = 5;

        // Act
        var result = controller.Delete(userId);

        // Assert
        _userService.Verify(s => s.Delete(userId), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void Delete_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var controller = CreateController();
        _userService.Setup(s => s.Delete(It.IsAny<int>())).Throws(new Exception("Something went wrong"));

        // Act
        var action = () => controller.Delete(1);

        // Assert
        action.Should().Throw<Exception>().WithMessage("Something went wrong");
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string dateOfBirth = "01/01/2000", string email = "juser@example.com", bool isActive = true)
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
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
