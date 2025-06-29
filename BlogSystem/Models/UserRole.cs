public enum UserRole
{
    Author,
    Editor,
    Admin
}

public static class RoleNames
{
    public const string Author = nameof(UserRole.Author);
    public const string Editor = nameof(UserRole.Editor);
    public const string Admin = nameof(UserRole.Admin);
}
