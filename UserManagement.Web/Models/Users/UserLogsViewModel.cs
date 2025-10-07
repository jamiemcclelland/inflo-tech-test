using UserManagement.Models;

namespace UserManagement.Web.Models.Users
{
    public class UserLogsViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<UserLog> Logs { get; set; } = new();
    }
}
