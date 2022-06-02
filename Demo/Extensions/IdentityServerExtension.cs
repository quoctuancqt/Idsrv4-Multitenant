using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Demo.Data;
using Demo.Identity;
using Demo.Models;
using IdentityServer.Services;
using System.Linq;
using Demo.Security;
using Demo.Services;

namespace Demo.Extensions
{
    public static class IdentityServerExtension
    {
        public static IServiceCollection AddIdsrv4(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                SetPasswordRequiredOptions(options, environment);
            })
                .AddSignInManager<ApplicationSignInManager>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserValidator<ApplicationUserValidator>();

            //Remove this Validator because we don´t need to check for a unique username! The Correct validation is in the class ApplicationUserValidator        
            var userValidator =
                services.FirstOrDefault(s => s.ImplementationType == typeof(UserValidator<ApplicationUser>));
            if (userValidator != null)
            {
                services.Remove(userValidator);
            }

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction.LoginUrl = "/Account/Login";
                options.UserInteraction.LogoutUrl = "/Account/Logout";
                options.UserInteraction.ConsentUrl = "/Consent/Index";
            })
                .AddConfigurationStore<ApplicationDbContext>(options =>
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(connectionString);
                })
                .AddOperationalStore<ApplicationDbContext>(options =>
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(connectionString);
                    options.EnableTokenCleanup = true;
                })
                .AddProfileService<CustomProfileService>()
                .AddRedirectUriValidator<CustomRedirectValidator>()
                .AddAspNetIdentity<ApplicationUser>()
                .AddSigningCredential(configuration, environment);

            services.AddScoped<IProfileService, AppProfileService>();

            services.AddAuthentication()
                .AddLocalApi(options =>
                {
                    options.ExpectedScope = "oauth.api";
                });

            return services;
        }

        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                // Note: Don't use developer signing credential in Production
                builder.AddSigningCredential(default);
            }

            return builder;
        }

        private static void SetPasswordRequiredOptions(IdentityOptions identityOptions, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                SetPasswordRequiredOptionsForDevelopment(identityOptions);
                return;
            }

            identityOptions.Password.RequiredLength = 8;
            identityOptions.Password.RequiredUniqueChars = 2;
        }

        private static void SetPasswordRequiredOptionsForDevelopment(IdentityOptions identityOptions)
        {
            identityOptions.Password.RequireDigit = false;
            identityOptions.Password.RequiredLength = 2;
            identityOptions.Password.RequiredUniqueChars = 0;
            identityOptions.Password.RequireLowercase = false;
            identityOptions.Password.RequireNonAlphanumeric = false;
            identityOptions.Password.RequireUppercase = false;
        }
    }
}
