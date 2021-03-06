using System;
using CoreMultiTenancy.Identity.Entities;
using CoreMultiTenancy.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoreMultiTenancy.Identity.Controllers
{
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IOrganizationManager _orgManager;

        public PermissionsController(UserManager<User> userManager,
            IOrganizationManager orgManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
        }

        [HttpGet]
        public IActionResult Get(string orgId)
        {
            return Ok();
        }
    }
}