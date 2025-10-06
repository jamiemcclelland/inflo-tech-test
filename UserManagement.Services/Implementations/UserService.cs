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
    }

    public void Delete(int id)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _dataAccess.Delete(user);
        }
    }

    public void Update(int id, User updatedUser)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.Forename = updatedUser.Forename;
            user.Surname = updatedUser.Surname;
            user.DateOfBirth = updatedUser.DateOfBirth;
            user.Email = updatedUser.Email;
            user.IsActive = updatedUser.IsActive;

            _dataAccess.Update(user);
        }
    }
}
