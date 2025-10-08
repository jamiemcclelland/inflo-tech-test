using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public IEnumerable<User> FilterByActive(bool isActive)
    {
        return _dataAccess.GetAll<User>().Where(u => u.IsActive == isActive).ToList();
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>().ToList();

    public void Add(User user)
    {
        _dataAccess.Create(user);

        var log = new UserLog
        {
            UserId = user.Id,
            Action = "Created user",
            PreviousValue = string.Empty,
            NewValue = $"User {user.Forename} {user.Surname} created.",
            Timestamp = DateTime.UtcNow,
            UserName =  $"{user.Forename} {user.Surname}"
        };

        _dataAccess.Create(log);
    }

    public void Delete(int id)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _dataAccess.Delete(user);

            var log = new UserLog
            {
                UserId = user.Id,
                Action = "Deleted user",
                PreviousValue = $"User {user.Forename} {user.Surname} ({user.Email}) deleted.",
                NewValue = string.Empty,
                Timestamp = DateTime.UtcNow,
                UserName = $"{user.Forename} {user.Surname}"
            };

            _dataAccess.Create(log);
        }
    }

    public void Update(int id, User updatedUser)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            var logs = new List<UserLog>();

            if (user.Forename != updatedUser.Forename)
                logs.Add(new UserLog { UserId = id, Action = "Forename changed", PreviousValue = user.Forename, NewValue = updatedUser.Forename, UserName =  $"{updatedUser.Forename} {updatedUser.Surname}" });

            if (user.Surname != updatedUser.Surname)
                logs.Add(new UserLog { UserId = id, Action = "Surname changed", PreviousValue = user.Surname, NewValue = updatedUser.Surname, UserName =  $"{updatedUser.Forename} {updatedUser.Surname}" });

            if (user.DateOfBirth != updatedUser.DateOfBirth)
                logs.Add(new UserLog { UserId = id, Action = "Date of birth changed", PreviousValue = user.DateOfBirth, NewValue = updatedUser.DateOfBirth, UserName =  $"{updatedUser.Forename} {updatedUser.Surname}" });

            if (user.Email != updatedUser.Email)
                logs.Add(new UserLog { UserId = id, Action = "Email changed", PreviousValue = user.Email, NewValue = updatedUser.Email, UserName =  $"{updatedUser.Forename} {updatedUser.Surname}" });

            if (user.IsActive != updatedUser.IsActive)
                logs.Add(new UserLog { UserId = id, Action = "Active status changed", PreviousValue = user.IsActive.ToString(), NewValue = updatedUser.IsActive.ToString(), UserName =  $"{updatedUser.Forename} {updatedUser.Surname}" });


            user.Forename = updatedUser.Forename;
            user.Surname = updatedUser.Surname;
            user.DateOfBirth = updatedUser.DateOfBirth;
            user.Email = updatedUser.Email;
            user.IsActive = updatedUser.IsActive;

            _dataAccess.Update(user);

            foreach (var log in logs)
            {
                _dataAccess.Create(log);
            }
        }
    }
}
