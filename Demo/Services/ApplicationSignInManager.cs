using Demo.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Demo.Services
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        private readonly IUserService _userService;

        public ApplicationSignInManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation, IUserService userService)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _userService = userService;
        }

        public async Task<SignInResult> PasswordSignInAsync(string userName, string password, string tenant, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await _userService.GetUserAsync(userName, tenant);

            if (user is null) return SignInResult.Failed;

            return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }
    }
}
