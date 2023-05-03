using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ES.Labs.Api.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, SampleAuthorizationHandler>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        services
            .AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAssertion(context => true)
                    .Build();

                options.AddPolicy(Policies.AdminRole, policy =>
                    policy.Requirements.Add(new HasAnyRoleRequirement("Admin")));

                options.AddPolicy(Policies.OnlyEvenSeconds, ConfigurePolicy);
            });
            
        return services;
    }

    private static void ConfigurePolicy(AuthorizationPolicyBuilder obj)
    {
        obj.RequireAssertion(context => DateTime.Now.Second % 2 == 0);
    }
}

public class SampleAuthorizationHandler : AuthorizationHandler<HasAnyRoleRequirement>
{
    private readonly ILogger _logger;

    public SampleAuthorizationHandler(ILoggerFactory loggerFactory)
        => _logger = loggerFactory.CreateLogger(GetType().FullName);

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, HasAnyRoleRequirement requirement)
    {
        _logger.LogInformation("Inside my handler at ");

        _logger.LogInformation($"Yead {context.Resource}");

        // context.Fail();
        context.Succeed(requirement);
        // ...

        return Task.CompletedTask;
    }
}