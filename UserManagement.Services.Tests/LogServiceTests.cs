using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Service.Tests
{
    public class UserLogServiceTests
    {
        private readonly Mock<IDataContext> _dataContext = new();
        private UserLogService CreateService() => new(_dataContext.Object);

        [Fact]
        public void GetLogsForUser_ShouldReturnOnlyLogsForSpecifiedUser()
        {
            // Arrange
            var service = CreateService();
            var logs = new List<UserLog>
            {
                new UserLog { LogId = 1, UserId = 1, Action = "Created user", Timestamp = DateTime.UtcNow },
                new UserLog { LogId = 2, UserId = 2, Action = "Email changed", Timestamp = DateTime.UtcNow },
                new UserLog { LogId = 3, UserId = 1, Action = "Forename changed", Timestamp = DateTime.UtcNow }
            };

            _dataContext.Setup(d => d.GetAll<UserLog>()).Returns(logs);

            // Act
            var result = service.GetLogsForUser(1).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(l => l.UserId == 1);
        }

        [Fact]
        public void GetLogsForUser_ShouldReturnLogsInDescendingOrder()
        {
            // Arrange
            var service = CreateService();
            var now = DateTime.UtcNow;
            var logs = new List<UserLog>
            {
                new UserLog { LogId = 1, UserId = 1, Action = "Created user", Timestamp = now.AddMinutes(-10) },
                new UserLog { LogId = 2, UserId = 1, Action = "Email changed", Timestamp = now.AddMinutes(-5) },
                new UserLog { LogId = 3, UserId = 1, Action = "Forename changed", Timestamp = now }
            };

            _dataContext.Setup(d => d.GetAll<UserLog>()).Returns(logs);

            // Act
            var result = service.GetLogsForUser(1).ToList();

            // Assert
            result.Should().BeInDescendingOrder(l => l.Timestamp);
            result.First().Action.Should().Be("Forename changed");
            result.Last().Action.Should().Be("Created user");
        }

        [Fact]
        public void AddLog_ShouldCallDataContextCreateWithLog()
        {
            // Arrange
            var service = CreateService();
            var log = new UserLog
            {
                LogId = 1,
                UserId = 1,
                Action = "Test action",
                Timestamp = DateTime.UtcNow
            };

            // Act
            service.AddLog(log);

            // Assert
            _dataContext.Verify(d => d.Create(It.Is<UserLog>(l => l == log)), Times.Once);
        }

        [Fact]
        public void GetLogsForUser_WhenNoLogsExist_ShouldReturnEmpty()
        {
            // Arrange
            var service = CreateService();
            _dataContext.Setup(d => d.GetAll<UserLog>()).Returns(new List<UserLog>());

            // Act
            var result = service.GetLogsForUser(999).ToList();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAllLogs_ShouldReturnAllLogsInDescendingOrder()
        {
            // Arrange
            var service = CreateService();
            var now = DateTime.UtcNow;
            var logs = new List<UserLog>
            {
                new UserLog { LogId = 1, UserId = 1, Action = "Created user", Timestamp = now.AddMinutes(-10) },
                new UserLog { LogId = 2, UserId = 2, Action = "Email changed", Timestamp = now.AddMinutes(-5) },
                new UserLog { LogId = 3, UserId = 1, Action = "Forename changed", Timestamp = now }
            };

            _dataContext.Setup(d => d.GetAll<UserLog>()).Returns(logs);

            // Act
            var result = service.GetAllLogs().ToList();

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeInDescendingOrder(l => l.Timestamp);
            result.First().Action.Should().Be("Forename changed");
            result.Last().Action.Should().Be("Created user");
        }
    }
}
