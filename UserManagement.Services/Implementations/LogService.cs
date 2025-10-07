using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations
{
    public class UserLogService : IUserLogService
    {
        private readonly IDataContext _dataContext;

        public UserLogService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IEnumerable<UserLog> GetLogsForUser(int userId)
        {
            return _dataContext.GetAll<UserLog>()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }

        public void AddLog(UserLog log)
        {
            _dataContext.Create(log);
        }

        public IEnumerable<UserLog> GetAllLogs()
        {
            return _dataContext.GetAll<UserLog>()
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }
    }
}
