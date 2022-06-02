using IdentityModel;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Demo.Models;
using System;
using System.Linq;
using System.Security.Claims;
using IdentityServer.Services;
using Core.Models;

namespace Demo.Data
{
    public class ApplicationDbSeeder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly TenantDbContext _tenantContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRegistrationService _registrationService;

        public ApplicationDbSeeder(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IRegistrationService registrationService,
            TenantDbContext tenantContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _registrationService = registrationService;
            _tenantContext = tenantContext;
            _dbContext.Database.Migrate();
            _tenantContext.Database.Migrate();
        }

        public void EnsureData()
        {
            if (!_dbContext.Roles.Any())
            {
                var roles = Roles.GetAllRoles();
                foreach (var role in roles)
                {
                    _ = _roleManager.CreateAsync(new IdentityRole { Name = role, NormalizedName = role.ToUpper() }).Result;
                }
            }

            var alice = _userManager.FindByNameAsync("alice@tenant1.com").Result;

            if (alice is null)
            {
                var result = _registrationService.CreateTenantAndUserAsync("alice@tenant1.com", "alice@123", "Tenant 1").Result;

                if (result.ResultType != RegistrationResultType.Success)
                {
                    throw new Exception(result.IdentityResult.Errors.First().Description);
                }

                alice = _userManager.FindByNameAsync("alice@tenant1.com").Result;

                var identityResult = _userManager.AddToRoleAsync(alice, Roles.Administrator).Result;

                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }

                identityResult = _userManager.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com")
                        }).Result;

                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }
            }

            var bob = _userManager.FindByNameAsync("bob@tenant2.com").Result;

            if (bob is null)
            {
                var result = _registrationService.CreateTenantAndUserAsync("bob@tenant2.com", "bob@123", "Tenant 2").Result;

                if (result.ResultType != RegistrationResultType.Success)
                {
                    throw new Exception(result.IdentityResult.Errors.First().Description);
                }

                bob = _userManager.FindByNameAsync("bob@tenant2.com").Result;

                var identityResult = _userManager.AddToRoleAsync(bob, Roles.Administrator).Result;

                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }

                identityResult = _userManager.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com")
                        }).Result;

                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }
            }

            if (!_dbContext.Clients.Any())
            {
                foreach (var client in Config.Clients)
                    _dbContext.Clients.Add(client.ToEntity());

                _dbContext.SaveChanges();
            }

            if (!_dbContext.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                    _dbContext.IdentityResources.Add(resource.ToEntity());

                _dbContext.SaveChanges();
            }

            if (!_dbContext.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    var entity = resource.ToEntity();
                    entity.Required = true;
                    _dbContext.ApiScopes.Add(entity);
                }


                _dbContext.SaveChanges();
            }

            if (!_dbContext.ApiResources.Any())
            {
                foreach (var apiResource in Config.GetApis())
                    _dbContext.ApiResources.Add(apiResource.ToEntity());

                _dbContext.SaveChanges();
            }
        }
    }
}
