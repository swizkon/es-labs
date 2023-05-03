using Microsoft.AspNetCore.Authorization;

namespace ES.Labs.Api.Security;

public class HasAnyRoleRequirement : IAuthorizationRequirement
{
    public HasAnyRoleRequirement(string roles)
    {
        Roles = roles;
    }

    public string Roles { get; }
}