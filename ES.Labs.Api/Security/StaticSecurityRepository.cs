namespace ES.Labs.Api.Security;

public class StaticSecurityRepository : ISecurityRepository
{

    const string AdminRole = "AdminRole";
    const string SystemAdmin = "SystemAdmin";
    const string BP_ADMIN = "BP_ADMIN";

    public static IEnumerable<string> AllRoles => new List<string>
    {
        AdminRole,
        SystemAdmin,
        BP_ADMIN
    };

    public async Task<IEnumerable<string>> GetAvailableRoles()
        => await Task.FromResult(AllRoles);
}