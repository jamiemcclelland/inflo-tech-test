using System.Linq;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            DateOfBirth = "01/01/2000",
            Email = "brandnewuser@example.com"
        };
        context.Create(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public void GetAll_WhenNewLogAdded_MustIncludeLog()
    {
        var context = CreateContext();

        var log = new UserLog
        {
            UserId = 1,
            Action = "Test Action",
            PreviousValue = "Old",
            NewValue = "New"
        };

        context.Create(log);

        var result = context.GetAll<UserLog>();

        result.Should().Contain(l => l.Action == "Test Action")
              .Which.Should().BeEquivalentTo(log, opts => opts.Excluding(l => l.LogId));
    }

    [Fact]
    public void GetAll_WhenLogDeleted_MustNotIncludeDeletedLog()
    {
        var context = CreateContext();
        var log = new UserLog
        {
            UserId = 1,
            Action = "To be deleted",
            PreviousValue = "Old",
            NewValue = "New"
        };
        context.Create(log);

        context.Delete(log);

        var result = context.GetAll<UserLog>();

        result.Should().NotContain(l => l.Action == "To be deleted");
    }

    [Fact]
    public void SeededUsers_ShouldBePresent()
    {
        var context = CreateContext();
        var users = context.GetAll<User>();

        users.Should().NotBeNullOrEmpty();
        users.Should().Contain(u => u.Forename == "Peter");
        users.Should().HaveCountGreaterThan(5);
    }

    private DataContext CreateContext() => new();
}
