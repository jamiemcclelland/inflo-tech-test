namespace UserManagement.Web.Models.Users;

public class UserCreateViewModel
{
    public List<UserCreateItemViewModel> Items { get; set; } = new();
}

public class UserCreateItemViewModel
{
    public string? Forename { get; set; }
    public string? Surname { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Email { get; set; }
}
