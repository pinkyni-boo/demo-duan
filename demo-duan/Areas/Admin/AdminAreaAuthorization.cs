using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;

namespace demo_duan.Areas.Admin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AdminAreaAuthorization : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                // Người dùng chưa đăng nhập
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new { area = "Identity", page = "/Account/Login", returnUrl = context.HttpContext.Request.Path }));
                return;
            }

            if (!context.HttpContext.User.IsInRole("Admin"))
            {
                // Người dùng đã đăng nhập nhưng không phải Admin
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new { controller = "Home", action = "AccessDenied", area = "" }));
            }
        }
    }
}