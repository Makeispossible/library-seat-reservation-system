using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibrarySeatReservation.Web.Filters;

/// <summary>
/// 管理端 Session 认证过滤器
/// 检查 Session 中 AdminLoggedIn 是否存在，否则重定向到登录页
/// </summary>
public class AdminAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetString("AdminLoggedIn") != "true")
        {
            context.Result = new RedirectToActionResult("Login", "Admin", null);
        }
    }
}
