using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Security.Principal;


namespace WebApplication3.Extentions
{
    public static class IdentityExtensions
    {
        public static string GetCompanyName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("CompanyName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetCompanyId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("CompanyId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}