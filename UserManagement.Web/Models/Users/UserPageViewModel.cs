using UserManagement.Web.Models.Users;

public class UserPageViewModel
{
    // For listing users
    public List<UserListItemViewModel> Users { get; set; } = new();

    // For the sidebar “Add User” form
    public UserCreateViewModel User { get; set; } = new();
}
