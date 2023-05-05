using Microsoft.AspNetCore.Authorization;

namespace ES.Labs.Api.Security;

public class HasPermissionRequirement : IAuthorizationRequirement
{
    public Permissions Permissions { get; }

    public HasPermissionRequirement(Permissions permissions)
    {
        Permissions = permissions;
    }
}