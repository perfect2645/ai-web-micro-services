using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using service.file.Services;

namespace service.file.Filters.Files
{
    public class FileKeyValidationFilterAttribute : ActionFilterAttribute
    {
        private readonly IFileUploadService _fileUploadService;
        public FileKeyValidationFilterAttribute(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var fileSize = context.ActionArguments["fileSize"] as long?;
            if (!fileSize.HasValue)
            {
                return;
            }
            var fileHash = context.ActionArguments["sha256Hash"] as string;
            if (string.IsNullOrWhiteSpace(fileHash))
            {
                return;
            }

            bool fileSizeValid = ValidateFileSize(context, fileSize.Value);
            if (!fileSizeValid)
            {
                return;
            }

            bool fileHashValid = ValidateFileHash(context, fileHash);
            if (!fileHashValid)
            {
                return;
            }

            var targetFileItem = await _fileUploadService.GetUploadedItemAsync(fileSize.Value, fileHash);
            if (targetFileItem == null)
            {
                context.ModelState.AddModelError("file", $"File doesn't exist.");
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "File not found",
                    Detail = $"File with size=[{fileSize.Value}], hash=[{fileHash}] doesn't exist."
                };
                context.Result = new NotFoundObjectResult(problemDetails);
                return;
            }

            context.HttpContext.Items.Add("targetFileItem", targetFileItem);

            await next();
        }

        private bool ValidateFileHash(ActionExecutingContext context, string? fileHash)
        {
            if (string.IsNullOrWhiteSpace(fileHash) || fileHash.Length != 64)
            {
                context.ModelState.AddModelError("sha256Hash", "File hash format is invalid.");
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid file hash",
                    Detail = "The provided file hash is invalid."
                };
                context.Result = new BadRequestObjectResult(problemDetails);
                return false;
            }

            return true;
        }

        private bool ValidateFileSize(ActionExecutingContext context, long fileSize)
        {
            if (fileSize >= 0)
            {
                return true;
            }
            context.ModelState.AddModelError("fileSize", "File size must be non-negative.");
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid file size",
                Detail = "The provided file size is invalid."
            };

            context.Result = new BadRequestObjectResult(problemDetails);
            return false;
        }
    }
}
