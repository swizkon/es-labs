using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IFeatureManager _featureManager;
        private readonly ILogger<IndexModel> _logger;

        public string FlagData { get; set; }

        public IndexModel(
            IFeatureManager featureManager,
            ILogger<IndexModel> logger)
        {
            _featureManager = featureManager;
            _logger = logger;
        }

        public async Task OnGet()
        {
            var myFlag = await _featureManager.IsEnabledAsync("MyFlag");
            
            var isAl = false; // await _featureManager.IsEnabledAsync("PermissionEnabled", new ParameterMatchContext("CallMe", "AL"));
            var isOther = false; // await _featureManager.IsEnabledAsync("PermissionEnabled", new ParameterMatchContext("CallMedfgdgfg", "Other"));

            var isLawyerOther = false; // await _featureManager.IsEnabledAsync("PermissionEnabled", new ParameterMatchContext("Lawyer", "Other"));
            var isLawyerSaul = await _featureManager.IsEnabledAsync("PermissionEnabled", new ParameterMatchContext("Lawyer", "Saul"));

            Func<string, bool> dd = s => s == "PermissionEnabled";
            var isYolo = await _featureManager.IsEnabledAsync("PermissionEnabled", dd);

            var isYear = await _featureManager.IsEnabledAsync("PermissionEnabled", DateTimeOffset.UtcNow);

            ViewData["FlagInfo"] = $"MyFlag: {myFlag} isAl: {isAl}  isOther: {isOther}  isLawyerOther: {isLawyerOther} isLawyerSaul: {isLawyerSaul}  isYolo: {isYolo}  isYear: {isYear}";

            await foreach (var featureName in _featureManager.GetFeatureNamesAsync())
            {
                _logger.LogWarning("featureName {featureName}", featureName);
            }
        }
    }
}