namespace ES.Labs.Api.Security;

public static class Policies
{
    public const string OnlyEvenSeconds = "OnlyEvenSeconds";
    public const string AdminRole = "AdminRole";
    public const string CanAccesAdmin = "CanAccesAdmin";

    public static IEnumerable<string> AllPolicies => new List<string>()
    {
        OnlyEvenSeconds,
        AdminRole,
        nameof(Permissions.AdminToolAccess)
    };
}