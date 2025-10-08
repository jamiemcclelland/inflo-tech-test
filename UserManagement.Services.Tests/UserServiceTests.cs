using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Service.Tests;

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
        result.Should().BeEquivalentTo(users, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public void FilterByActive_WhenTrue_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new List<User>
        {
            new User { Forename = "ActiveUser", IsActive = true },
            new User { Forename = "InactiveUser", IsActive = false }
        };

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
        var users = new List<User>
        {
            new User { Forename = "ActiveUser", IsActive = true },
            new User { Forename = "InactiveUser", IsActive = false }
        };

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.FilterByActive(false).ToList();

        // Assert
        result.Should().ContainSingle()
              .Which.Forename.Should().Be("InactiveUser");
    }

    [Fact]
    public void Add_WhenCalled_CallsCreateOnDataContext()
    {
        // Arrange
        var service = CreateService();
        var newUser = new User { Forename = "NewUser", IsActive = true };

        // Act
        service.Add(newUser);

        // Assert
        _dataContext.Verify(d => d.Create(newUser), Times.Once);
    }

    [Fact]
    public void Delete_WhenUserExists_ShouldCallDeleteAndCreateDeleteLog()
    {
        // Arrange
        var service = CreateService();
        var user = new User
        {
            Id = 1,
            Forename = "Test",
            Surname = "User",
            Email = "test@example.com"
        };
        var users = new List<User> { user };

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        service.Delete(1);

        // Assert
        _dataContext.Verify(s => s.Delete(user), Times.Once);

        _dataContext.Verify(s => s.Create(It.Is<UserLog>(log =>
        log.UserId == user.Id &&
        log.Action == "Deleted user" &&
        log.PreviousValue.Contains("Test User") &&
        log.NewValue == string.Empty &&
        log.UserName == "Test User"
    )), Times.Once);
    }

    [Fact]
    public void Delete_WhenUserDoesNotExist_ShouldNotCallDelete()
    {
        // Arrange
        var service = CreateService();
        var users = new List<User> { new User { Id = 1, Forename = "Test", Surname = "User" } };

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        service.Delete(2);

        // Assert
        _dataContext.Verify(s => s.Delete(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Update_ExistingUser_UpdatesPropertiesAndCallsDataAccessUpdate()
    {
        var service = CreateService();
        var existingUser = new User
        {
            Id = 1,
            Forename = "OldFirst",
            Surname = "OldLast",
            DateOfBirth = "01/01/1990",
            Email = "old@example.com",
            IsActive = true
        };

        var updatedUser = new User
        {
            Forename = "NewFirst",
            Surname = "NewLast",
            DateOfBirth = "02/02/2000",
            Email = "new@example.com",
            IsActive = false
        };

        var users = new List<User> { existingUser };
        _dataContext.Setup(d => d.GetAll<User>()).Returns(users);

        service.Update((int)existingUser.Id, updatedUser);

        existingUser.Forename.Should().Be("NewFirst");
        existingUser.Surname.Should().Be("NewLast");
        existingUser.DateOfBirth.Should().Be("02/02/2000");
        existingUser.Email.Should().Be("new@example.com");
        existingUser.IsActive.Should().BeFalse();

        _dataContext.Verify(d => d.Update(existingUser), Times.Once);
    }

    [Fact]
    public void Update_NonExistentUser_DoesNotCallDataAccessUpdate()
    {
        var service = CreateService();
        var updatedUser = new User
        {
            Forename = "NewFirst",
            Surname = "NewLast",
            DateOfBirth = "02/02/2000",
            Email = "new@example.com",
            IsActive = false
        };

        _dataContext.Setup(d => d.GetAll<User>()).Returns(new List<User>());
        service.Update(999, updatedUser);
        _dataContext.Verify(d => d.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Update_WithPartialNullProperties_UpdatesOnlyProvidedProperties()
    {
        var service = CreateService();
        var existingUser = new User
        {
            Id = 1,
            Forename = "OldFirst",
            Surname = "OldLast",
            DateOfBirth = "01/01/1990",
            Email = "old@example.com",
            IsActive = true
        };

        var updatedUser = new User
        {
            Forename = string.Empty,
            Surname = "UpdatedLast",
            DateOfBirth = string.Empty,
            Email = "updated@example.com",
            IsActive = false
        };

        var users = new List<User> { existingUser };
        _dataContext.Setup(d => d.GetAll<User>()).Returns(users);

        service.Update((int)existingUser.Id, updatedUser);

        existingUser.Forename.Should().Be(string.Empty);
        existingUser.Surname.Should().Be("UpdatedLast");
        existingUser.DateOfBirth.Should().Be(string.Empty);
        existingUser.Email.Should().Be("updated@example.com");
        existingUser.IsActive.Should().BeFalse();

        _dataContext.Verify(d => d.Update(existingUser), Times.Once);
    }

    [Fact]
    public void Add_ShouldCreateUserAndLogCreation()
    {
        // Arrange
        var service = CreateService();
        var newUser = new User
        {
            Id = 99,
            Forename = "Peter",
            Surname = "Parker",
            Email = "spidey@example.com",
            DateOfBirth = "01/01/2000",
            IsActive = true
        };

        // Act
        service.Add(newUser);

        // Assert
        _dataContext.Verify(d => d.Create(It.Is<User>(u => u == newUser)), Times.Once);

        _dataContext.Verify(d => d.Create(It.Is<UserLog>(log =>
            log.UserId == newUser.Id &&
            log.Action == "Created user" &&
            log.NewValue.Contains("Peter Parker") &&
            log.PreviousValue == string.Empty &&
            log.UserName == "Peter Parker"
        )), Times.Once);
    }

    [Fact]
    public void Update_WhenUserPropertiesChange_ShouldCreateLogsForEachChange()
    {
        // Arrange
        var service = CreateService();
        var existingUser = new User
        {
            Id = 1,
            Forename = "OldFirst",
            Surname = "OldLast",
            DateOfBirth = "01/01/1990",
            Email = "old@example.com",
            IsActive = true
        };

        var updatedUser = new User
        {
            Forename = "NewFirst",
            Surname = "NewLast",
            DateOfBirth = "02/02/2000",
            Email = "new@example.com",
            IsActive = false
        };

        _dataContext.Setup(d => d.GetAll<User>()).Returns(new List<User> { existingUser });

        // Act
        service.Update((int)existingUser.Id, updatedUser);

        // Assert
        _dataContext.Verify(d => d.Update(existingUser), Times.Once);

        _dataContext.Verify(d => d.Create(It.Is<UserLog>(l =>
            l.UserId == existingUser.Id &&
            l.Action == "Forename changed" &&
            l.PreviousValue == "OldFirst" &&
            l.NewValue == "NewFirst" &&
            l.UserName == "NewFirst NewLast"
        )), Times.Once);

        _dataContext.Verify(d => d.Create(It.Is<UserLog>(l =>
            l.UserId == existingUser.Id &&
            l.Action == "Active status changed" &&
            l.PreviousValue == "True" &&
            l.NewValue == "False"
        )), Times.Once);

        _dataContext.Verify(d => d.Create(It.IsAny<UserLog>()), Times.Exactly(5));
    }

    [Fact]
    public void Update_WhenNoPropertiesChange_ShouldNotCreateAnyLogs()
    {
        // Arrange
        var service = CreateService();
        var existingUser = new User
        {
            Id = 1,
            Forename = "Same",
            Surname = "Same",
            DateOfBirth = "01/01/2000",
            Email = "same@example.com",
            IsActive = true
        };

        var updatedUser = new User
        {
            Forename = "Same",
            Surname = "Same",
            DateOfBirth = "01/01/2000",
            Email = "same@example.com",
            IsActive = true
        };

        _dataContext.Setup(d => d.GetAll<User>()).Returns(new List<User> { existingUser });

        // Act
        service.Update((int)existingUser.Id, updatedUser);

        // Assert
        _dataContext.Verify(d => d.Update(existingUser), Times.Once);
        _dataContext.Verify(d => d.Create(It.IsAny<UserLog>()), Times.Never);
    }

    [Fact]
    public void Delete_WhenUserExists_ShouldCreateDeletedUserLog()
    {
        // Arrange
        var service = CreateService();
        var user = new User
        {
            Id = 5,
            Forename = "Tony",
            Surname = "Stark",
            Email = "ironman@avengers.com"
        };
        var users = new List<User> { user };

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        service.Delete((int)user.Id);

        // Assert: ensures delete is called
        _dataContext.Verify(s => s.Delete(user), Times.Once);

        // Assert: ensures a deletion log is created
        _dataContext.Verify(s => s.Create(It.Is<UserLog>(log =>
            log.UserId == user.Id &&
            log.Action == "Deleted user" &&
            log.PreviousValue.Contains("Tony Stark") &&
            log.NewValue == string.Empty &&
            log.UserName == "Tony Stark"
        )), Times.Once);
    }

    private List<User> SetupUsers(string forename = "Johnny", string surname = "User", string dateOfBirth = "01/01/2000", string email = "juser@example.com", bool isActive = true)
    {
        var users = new List<User>
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

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
