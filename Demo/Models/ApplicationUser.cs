using Microsoft.AspNetCore.Identity;

namespace Demo.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string TenantId { get; set; }
    }
}
