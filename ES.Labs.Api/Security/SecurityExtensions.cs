using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ES.Labs.Api.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<ISecurityRepository, StaticSecurityRepository>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
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
                    .RequireAssertion(_ => true)
                    .Build();
                
                // TODO Fix better parsing...
                foreach (var permissionName in Enum.GetNames<Permissions>())
                {
                    var permission = (Permissions)Enum.Parse(typeof(Permissions), permissionName);
                    options.AddPolicy(permissionName, policy =>
                        policy.Requirements.Add(new HasPermissionRequirement(permission)));
                }

                foreach (var policy in Policies.AllPolicies)
                {
                    options.AddPolicy(policy.Key, policy.Value);
                }
            });
            
        return services;
    }
}