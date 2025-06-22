public class UserStore
{
    public static List<User> Users = new List<User>
    {
        new User {
            Id = 1,
            Username = "admin",
            Email = "jana@test.com",
            PasswordHash = "try",
            Role = UserRole.Admin,
            }
    };
}