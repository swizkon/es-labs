using Microsoft.AspNetCore.Authorization;

namespace ES.Labs.Api.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<HasPermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionRequirement requirement)
    {
        _logger.LogInformation("Validate permission {Permissions}", requirement.Permissions);

        var req = (HttpContext?) context.Resource;
        var roles = req?.Request.Query["roles"];
        if (roles.GetValueOrDefault().Contains(requirement.Permissions.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}