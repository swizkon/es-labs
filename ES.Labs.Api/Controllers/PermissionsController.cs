using ES.Labs.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ES.Labs.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [AllowAnonymous]
    public class PermissionsController : ControllerBase
    {
        private readonly ISecurityRepository _securityRepository;
        private readonly ILogger<EqualizerController> _logger;

        public PermissionsController(
            ISecurityRepository securityRepository,
            ILogger<EqualizerController> logger)
        {
            _securityRepository = securityRepository;
            _logger = logger;
        }

        [HttpGet("admintool")]
        [Authorize(Policy = nameof(Permissions.AdminToolAccess))]
        public IActionResult GetAdmin()
            => Ok("Can access admin");

        [HttpGet("roles")]
        [Authorize(Policy = nameof(Permissions.ReadAvailableRoles))]
        public async Task<IActionResult> GetAvailableRoles()
            => Ok(await _securityRepository.GetAvailableRoles());

        [HttpGet("policies")]
        public IActionResult GetAvailablePolicies()
            => Ok(Enum.GetNames<Permissions>().OrderBy(x => x));
    }
}