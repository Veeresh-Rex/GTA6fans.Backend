using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;


namespace GTA6fans.Api.Attributes;
public class OptionalAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // If token is present and invalid, reject
        if (user?.Identity?.IsAuthenticated == false &&
            context.HttpContext.Request.Headers.ContainsKey("Authorization"))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
