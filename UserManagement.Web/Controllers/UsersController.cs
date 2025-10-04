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
            Users = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                Forename = u.Forename,
                Surname = u.Surname,
                DateOfBirth = u.DateOfBirth,
                Email = u.Email,
                IsActive = u.IsActive
            }).ToList()
        };

        return View(model);
    }

    [HttpPost("Users/Create")]
    public IActionResult Create(UserPageViewModel model)
    {
        var newUser = model.NewUser;

        if (!ModelState.IsValid)
        {
            var users = _userService.GetAll();
            var pageModel = new UserPageViewModel
            {
                Users = users.Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    Forename = u.Forename,
                    Surname = u.Surname,
                    DateOfBirth = u.DateOfBirth,
                    Email = u.Email,
                    IsActive = u.IsActive
                }).ToList(),
                NewUser = newUser
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

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _userService.Delete(id);
        return Ok();
    }
}
