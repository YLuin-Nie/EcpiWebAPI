using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;

namespace EcpiWebAPI.Attributes
{
    public class RestrictToUsersAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedUsers;

        public RestrictToUsersAttribute(params string[] allowedUsers)
        {
            _allowedUsers = allowedUsers ?? new string[0];
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;

            if (!_allowedUsers.Contains(userName, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

