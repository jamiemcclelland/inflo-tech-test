using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models.Users;

public class UserCreateViewModel
{
    [Required]
    public string Forename { get; set; } = null!;
    [Required]
    public string Surname { get; set; } = null!;
    [Required]
    [DataType(DataType.Date, ErrorMessage = "Please enter a valid date")]
    public string DateOfBirth { get; set; } = null!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
}
