using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.Models;

namespace UserManagement.WebMS.Controllers
{
    [Route("users/logs")]
    public class UserLogsController : Controller
    {
        private readonly IUserLogService _logService;
        private readonly IUserService _userService;

        public UserLogsController(IUserLogService logService, IUserService userService)
        {
            _logService = logService;
            _userService = userService;
        }

        [HttpGet("")]
        public IActionResult List(int? userId = null)
        {
            IEnumerable<UserLog> logs;
            string title;

            if (userId.HasValue)
            {
                var user = _userService.GetAll().FirstOrDefault(u => u.Id == userId.Value);
                if (user == null) return NotFound();

                logs = _logService.GetLogsForUser(userId.Value);
                title = $"Change logs for {user.Forename} {user.Surname}";
            }
            else
            {
                logs = _logService.GetAllLogs();
                title = "All change logs";
            }

            var model = new UserLogsViewModel
            {
                UserId = userId ?? 0,
                UserName = title,
                Logs = logs.ToList()
            };

            return View("List", model);
        }
    }
}
