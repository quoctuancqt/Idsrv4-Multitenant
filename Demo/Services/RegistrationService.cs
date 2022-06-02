using System;
using System.Threading.Tasks;
using Core.Services;
using Demo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Services
{
    public interface IRegistrationService
    {
        Task<RegistrationResult> CreateTenantAndUserAsync(string email, string password, string tenantName);
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly ITenantService _tenantService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public RegistrationService(ITenantService tenantService, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _tenantService = tenantService;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task<RegistrationResult> CreateTenantAndUserAsync(string email, string password, string tenantName)
        {
            if (string.IsNullOrEmpty(tenantName))
                tenantName = _tenantService.GetTenantName(_contextAccessor.HttpContext);

            //Check how to do this right!
            if (string.IsNullOrEmpty(tenantName))
                throw new Exception("Tenant must set by field or url!");

            var tenantId = await GetOrCreateTenantAsync(tenantName);

            if (string.IsNullOrEmpty(tenantId))
            {
                return new RegistrationResult()
                {
                    ResultType = RegistrationResultType.TenantNameNotAvailable
                };
            }

            var userResult = await CreateUserAsync(email, password, tenantId);

            if (userResult.Succeeded)
            {
                return new RegistrationResult() { ResultType = RegistrationResultType.Success, IdentityResult = userResult };
            }

            await _tenantService.RemoveTenantAsync(tenantId);

            return new RegistrationResult()
            {
                ResultType = RegistrationResultType.UserNotCreated,
                IdentityResult = userResult
            };
        }

        private async Task<string> GetOrCreateTenantAsync(string tenantName)
        {
            var tenantId = await _tenantService.GetTenantIdAsync(tenantName).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            var result = await _tenantService.CreateAsync(tenantName);

            return result;
        }

        private async Task<IdentityResult> CreateUserAsync(string email, string password, string tenantId)
        {
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TenantId = tenantId
            };

            // Create new User in the Database
            var identityResult = await _userManager.CreateAsync(newUser, password);
            return identityResult;
        }
    }
}