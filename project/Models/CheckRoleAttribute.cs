using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;  // Thêm dòng này vào


namespace project.Models
{
    public class CheckRoleAttribute : ActionFilterAttribute
{
    private readonly int[] _requiredRoles;

    public CheckRoleAttribute(params int[] requiredRoles)
    {
        _requiredRoles = requiredRoles;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var currentUserRole = filterContext.HttpContext.Session["UserRole"];

        if (currentUserRole == null || ((int)currentUserRole != 3 && !_requiredRoles.Contains((int)currentUserRole)))
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary
                {
                    { "controller", "Ad" },
                    { "action", "adlogin" }
                });
        }

        base.OnActionExecuting(filterContext);
    }
}


}