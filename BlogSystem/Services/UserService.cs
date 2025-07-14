using System.Text.Json;
using System.Threading.Tasks;

public class UserService
{
    private readonly IPasswordHelper _passwordHelper;

    public UserService(IPasswordHelper passwordHelper)
    {
        _passwordHelper = passwordHelper;
    }

    public async Task<User?> CreateUser(UserRegisterDTO userRegisterDTO)
    {
        string hash = _passwordHelper.HashPassword(new User(), userRegisterDTO.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = userRegisterDTO.Username.ToLower(),
            Email = userRegisterDTO.Email,
            PasswordHash = hash,
            Role = userRegisterDTO.Role
        };

        try
        {
            await FileStorageHandler.SaveUser(user);
            return user;
        }
        catch
        {
            return null;
        }
    }

    public async Task<User?> GetUser(UserLoginDTO userLoginDTO)
    {
        var user = await FileStorageHandler.GetUserByUsername(userLoginDTO.Username.ToLower());
        if (user == null)
        {
            return null;
        }
        bool isValid = _passwordHelper.VerifyPassword(user, user.PasswordHash, userLoginDTO.Password);
        if (!isValid)
        {
            return null;
        }
        return user;
    }
}
