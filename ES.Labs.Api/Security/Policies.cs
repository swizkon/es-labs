using Microsoft.AspNetCore.Authorization;

namespace ES.Labs.Api.Security;

public static class Policies
{
    public const string OnlyEvenSeconds = "OnlyEvenSeconds";

    public static Action<AuthorizationPolicyBuilder> OnlyEvenSecondsPolicy => builder =>
    {
        builder.RequireAssertion(_ => DateTime.Now.Second % 2 == 0);
    };


    public const string MustHaveSession = "MustHaveSession";

    public static Action<AuthorizationPolicyBuilder> MustHaveSessionPolicy => builder =>
    {
        builder.RequireAssertion(context =>
        {
            var req = (HttpContext?) context.Resource;

            return req?.Request.QueryString.HasValue ?? false;
        });
    };


    public static IDictionary<string, Action<AuthorizationPolicyBuilder>> AllPolicies =>
        new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
        {
            { OnlyEvenSeconds, OnlyEvenSecondsPolicy },
            { MustHaveSession, MustHaveSessionPolicy }
        };
}