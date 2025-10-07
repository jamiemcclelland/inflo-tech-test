using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.WebMS.Controllers;
using UserManagement.Web.Models.Users;

namespace UserManagement.Web.Tests.Controllers
{
    public class UserLogsControllerTests
    {
        private readonly Mock<IUserService> _userService = new();
        private readonly Mock<IUserLogService> _logService = new();
        private UserLogsController CreateController() => new(_logService.Object, _userService.Object);

        [Fact]
        public void List_UserExists_ReturnsViewWithModel()
        {
            // Arrange
            var controller = CreateController();
            var userId = 1;
            var user = new User { Id = userId, Forename = "John", Surname = "Doe" };
            var logs = new List<UserLog>
            {
                new UserLog { LogId = 1, UserId = userId, Action = "Created user" }
            };

            _userService.Setup(s => s.GetAll()).Returns(new List<User> { user });
            _logService.Setup(s => s.GetLogsForUser(userId)).Returns(logs);

            // Act
            var result = controller.List(userId);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<UserLogsViewModel>();

            var model = (UserLogsViewModel)viewResult.Model;
            model.UserId.Should().Be(userId);
            model.UserName.Should().Be("Change logs for John Doe");
            model.Logs.Should().BeEquivalentTo(logs);
        }

        [Fact]
        public void List_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController();
            var userId = 99;

            _userService.Setup(s => s.GetAll()).Returns(new List<User>());

            // Act
            var result = controller.List(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void List_NoLogsForUser_ReturnsViewWithEmptyLogs()
        {
            // Arrange
            var controller = CreateController();
            var userId = 1;
            var user = new User { Id = userId, Forename = "John", Surname = "Doe" };

            _userService.Setup(s => s.GetAll()).Returns(new List<User> { user });
            _logService.Setup(s => s.GetLogsForUser(userId)).Returns(new List<UserLog>());

            // Act
            var result = controller.List(userId);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = (UserLogsViewModel)viewResult!.Model!;
            model.Logs.Should().BeEmpty();
        }

        [Fact]
        public void List_NoUserId_ReturnsAllLogs()
        {
            // Arrange
            var controller = CreateController();
            var logs = new List<UserLog>
            {
                new UserLog { LogId = 1, UserId = 1, Action = "Created user" },
                new UserLog { LogId = 2, UserId = 2, Action = "Email changed" }
            };

            _logService.Setup(s => s.GetAllLogs()).Returns(logs);

            // Act
            var result = controller.List();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = (UserLogsViewModel)viewResult!.Model!;
            model.UserId.Should().Be(0);           // No specific user
            model.UserName.Should().Be("All change logs");
            model.Logs.Should().BeEquivalentTo(logs);
        }
    }
}
