using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class UserLog
    {
        [Key]
        public long LogId { get; set; }
        public long UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string PreviousValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
