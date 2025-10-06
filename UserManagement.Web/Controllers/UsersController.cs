using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("Users/List/{isActive?}")]
    public ViewResult List(bool? isActive = null)
    {
        IEnumerable<User> users;

        if (isActive.HasValue)
        {
            users = _userService.FilterByActive(isActive.Value);
        }
        else
        {
            users = _userService.GetAll();
        }

        var model = new UserPageViewModel
        {
            Users = users.Select(MapUserModels).ToList()
        };

        return View(model);
    }

    [HttpPost("Users/Create")]
    public IActionResult Create(UserPageViewModel model)
    {
        var newUser = model.User;

        if (!ModelState.IsValid)
        {
            var users = _userService.GetAll();
            var pageModel = new UserPageViewModel
            {
                Users = users.Select(MapUserModels).ToList(),
                User = newUser
            };

            return View("List", pageModel);
        }

        var formattedDate = DateTime.Parse(newUser.DateOfBirth).ToString("dd/MM/yyyy");

        var user = new User
        {
            Forename = newUser.Forename,
            Surname = newUser.Surname,
            DateOfBirth = formattedDate,
            Email = newUser.Email,
            IsActive = newUser.IsActive
        };

        _userService.Add(user);

        return RedirectToAction("List");
    }

    [HttpPost("Users/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, UserPageViewModel model)
    {
        var updatedUser = model.User;

        if (!ModelState.IsValid)
            return BadRequest("Invalid user data.");

        var formattedDate = DateTime.Parse(updatedUser.DateOfBirth).ToString("dd/MM/yyyy");

        var userToUpdate = new User
        {
            Id = id,
            Forename = updatedUser.Forename,
            Surname = updatedUser.Surname,
            DateOfBirth = formattedDate,
            Email = updatedUser.Email,
            IsActive = updatedUser.IsActive
        };

        _userService.Update(id, userToUpdate);
        return RedirectToAction("List");
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _userService.Delete(id);
        return Ok();
    }

    private static UserListItemViewModel MapUserModels(User user)
    {
        return new UserListItemViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            DateOfBirth = user.DateOfBirth,
            Email = user.Email,
            IsActive = user.IsActive
        };
    }
}
