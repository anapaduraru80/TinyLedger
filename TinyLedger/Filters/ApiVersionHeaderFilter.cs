using Microsoft.AspNetCore.Mvc.Filters;

namespace TinyLedger.Filters
{
    public class ApiVersionHeaderFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Append("API-Version", "1.0");
            base.OnActionExecuted(context);
        }
    }
}