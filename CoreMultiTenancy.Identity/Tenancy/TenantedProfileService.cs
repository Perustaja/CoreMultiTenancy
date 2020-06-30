using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreMultiTenancy.Identity.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreMultiTenancy.Identity.Tenancy
{
    public class TenantedProfileService : IProfileService
    {
        private readonly ILogger<TenantedProfileService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserClaimsPrincipalFactory<User> _principalsFactory;
        public TenantedProfileService(ILogger<TenantedProfileService> logger,
        UserManager<User> userManager,
        IUserClaimsPrincipalFactory<User> principalsFactory)
        {
            _logger = logger;
            _userManager = userManager;
            _principalsFactory = principalsFactory;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            foreach (var t in context.RequestedClaimTypes)
                _logger.LogInformation(t);
            
            context.LogProfileRequest(_logger);
            // Use the information we have to get the user associated with the subject and then its claims
            var subId = context.Subject.GetSubjectId();
            if (!Guid.TryParse(subId, out var parsedId))
                throw new Exception($"TenantedProfileService could not parse SubjectId: {subId} into usable Guid.");
            
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == parsedId) ?? throw new Exception($"Unable to find user with ID: {parsedId}.");
            var principal = await _principalsFactory.CreateAsync(user);

            // Append our custom claims
            var claims = principal.Claims.ToList();
            var tidClaim = new Claim("tid", user.SelectedOrg.ToString());
            claims.Add(tidClaim);
            
            context.AddRequestedClaims(claims);
            foreach (var c in context.IssuedClaims)
                _logger.LogInformation($"{c.Type}, {c.Value}");
        }
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subId = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(subId);

            context.IsActive = user != null;
        }
    }
}