using Microsoft.AspNetCore.Authorization;

public static class JwtPolicies
{
    public const string CanCreate = "CanCreate";
    public const string CanEdit = "CanEdit";
    public const string CanDelete = "CanDelete";
    public const string CanPublish = "CanPublish";

    public static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(CanCreate, policy =>
            policy.RequireRole(RoleNames.Author, RoleNames.Admin));

        options.AddPolicy(CanEdit, policy =>
            policy.RequireRole(RoleNames.Editor, RoleNames.Admin, RoleNames.Author));

        options.AddPolicy(CanDelete, policy =>
            policy.RequireRole(RoleNames.Admin, RoleNames.Author));

        options.AddPolicy(CanPublish, policy =>
            policy.RequireRole(RoleNames.Admin));
    }
}