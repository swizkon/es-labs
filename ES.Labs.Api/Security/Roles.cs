namespace ES.Labs.Api.Security;

public static class Roles
{
    public const string AdminRole = "AdminRole";

    public static IEnumerable<string> AllRoles => new List<string>()
    {
        AdminRole
    };
}