using System.Collections.Generic;

namespace Demo
{
    public static class Roles
    {
        public const string SuperAdministrator = "Super Administrator";
        public const string Administrator = "Administrator";

        public static IEnumerable<string> GetAllRoles()
        {
            yield return Administrator;
        }
    }
}
