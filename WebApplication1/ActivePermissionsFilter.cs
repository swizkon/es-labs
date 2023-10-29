using Microsoft.FeatureManagement;
using Microsoft.VisualBasic;

namespace WebApplication1;

//[FilterAlias("ActivePermissions")]
//public class ActivePermissionsFilter : IContextualFeatureFilter<ParameterMatchContext>
//{
//    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext featureFilterContext, ParameterMatchContext context)
//    {
//        var permissionConfig = featureFilterContext.Parameters[context.ParameterName];

//        var isEnabled = (permissionConfig ?? string.Empty).Equals(context.Value, StringComparison.InvariantCultureIgnoreCase);

//        return Task.FromResult(isEnabled);
//    }
//}

[FilterAlias("ActivePermissions")]
public class ActivePermissionsFilter :
    IContextualFeatureFilter<Func<string, bool>>
    // IContextualFeatureFilter<DateTimeOffset>
{
    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext featureFilterContext, Func<string, bool> context)
    {
        // var permissionConfig = featureFilterContext.Parameters[context.ParameterName];

        var isEnabled = context(featureFilterContext.FeatureName);// // (permissionConfig ?? string.Empty).Equals(context.Value, StringComparison.InvariantCultureIgnoreCase);

        return Task.FromResult(isEnabled);
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext featureFilterContext, DateTimeOffset context)
    {
        var isEnabled = context.Year == DateTimeOffset.UtcNow.Year;

        return Task.FromResult(isEnabled);
    }
}