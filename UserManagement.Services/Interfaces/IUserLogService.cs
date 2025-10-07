using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces
{
    public interface IUserLogService
    {
        IEnumerable<UserLog> GetLogsForUser(int userId);
        void AddLog(UserLog log);
        IEnumerable<UserLog> GetAllLogs();

    }
}
