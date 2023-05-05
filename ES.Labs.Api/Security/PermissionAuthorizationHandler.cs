using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ES.Labs.Api.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<HasPermissionRequirement>
{
    private readonly ILogger _logger;

    public PermissionAuthorizationHandler(ILoggerFactory loggerFactory)
        => _logger = loggerFactory.CreateLogger(GetType().FullName);

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionRequirement requirement)
    {
        _logger.LogInformation($"Validate permission {requirement.Permissions}");

        // _logger.LogInformation($"Yead {context.Resource}");
        
        // context.Fail();
        var req = (HttpContext?) context.Resource;
        var roles = req?.Request.Query["roles"];
        if (roles.GetValueOrDefault().Contains(requirement.Permissions.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}