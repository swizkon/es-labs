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
        private readonly ILogger<EqualizerController> _logger;

        public PermissionsController(
            ILogger<EqualizerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("admintool")]
        [Authorize(Policy = nameof(Permissions.AdminToolAccess))]
        public IActionResult GetAdmin()
            => Ok("Can access admin");

        [HttpGet("roles")]
        [Authorize(Policy = nameof(Permissions.ReadAvailableRoles))]
        public IActionResult GetAvailableRoles()
            => Ok(Roles.AllRoles);

        [HttpGet("policies")]
        public IActionResult GetAvailablePolicies()
            => Ok(Enum.GetNames<Permissions>().OrderBy(x => x));
    }
}