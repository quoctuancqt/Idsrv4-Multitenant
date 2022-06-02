using System;
using System.Threading.Tasks;
using Core.Services;
using Demo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserAsync(string userName, string tenantName);
    }
    
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;

        public UserService(UserManager<ApplicationUser> userManager, ITenantService tenantService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<ApplicationUser> GetUserAsync(string userName, string tenantName)
        {
            var tenantId = await _tenantService.GetTenantIdAsync(tenantName);
            return await GetUserByTenantIdAsync(userName, tenantId);
        }

        private async Task<ApplicationUser> GetUserByTenantIdAsync(string userName, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;
            if (string.IsNullOrEmpty(tenantId))
                return null;

            return await _userManager.Users.FirstOrDefaultAsync(u =>
                u.UserName == userName && u.TenantId == tenantId);
        }
    }
}