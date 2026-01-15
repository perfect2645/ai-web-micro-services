using Microsoft.AspNetCore.Mvc.Filters;

namespace service.file.Filters.Files
{
    public class FileUploadValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}
