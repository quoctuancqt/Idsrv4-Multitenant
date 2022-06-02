using Core.Models;
using Core.Services;
using Demo.Attributes;
using Demo.Data;
using Demo.Extensions;
using IdentityServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, configs => configs.MigrationsHistoryTable("__EFMigrationsHistory", "oauth")));

builder.Services.AddDbContext<TenantDbContext>(options =>
                options.UseNpgsql(connectionString, configs => configs.MigrationsHistoryTable("__EFMigrationsHistory", "oauth")));

builder.Services.AddScoped(serviceProvider =>
{
    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();
    return new ApplicationUserValidator(httpContext, tenantService);
});

// Add application services.
builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<ITenantService, TenantService>();
builder.Services.AddTransient<IRegistrationService, RegistrationService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddScoped<RequireTenantAttribute>();

builder.Services.AddTransient<ApplicationDbSeeder>();

builder.Services.AddIdsrv4(builder.Configuration, builder.Environment);

builder.Services.AddCors(configs =>
    configs.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader().WithOrigins(new string[] { "http://tenant1.local:4200", "http://tenant2.local:4200" });
    })
);

builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

var seeder = app.Services.CreateScope().ServiceProvider.GetService<ApplicationDbSeeder>();
seeder.EnsureData();

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    // In Production, use something more resilient
    scope.ServiceProvider.GetRequiredService<TenantDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
    IdentityModelEventSource.ShowPII = true;
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "Identity API V1");
    });
}
else
{
    app.UseCookiePolicy();
    app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
    app.UseExceptionHandler();
}

var forwardHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardHeadersOptions.KnownNetworks.Clear();
forwardHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardHeadersOptions);

app.UseCors("DefaultPolicy");

app.UseStaticFiles();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();