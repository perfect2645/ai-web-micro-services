using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using service.file.Configurations;
using service.file.Services;
using Utils.EncodingEx;

namespace service.file.Filters.Files
{
    public class FileUploadValidationFilterAttribute : ActionFilterAttribute
    {
        private IFileUploadService _fileUploadService;
        public FileUploadValidationFilterAttribute(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var fileItem = context.ActionArguments["file"] as IFormFile;
            if (fileItem == null || fileItem.Length == 0)
            {
                context.ModelState.AddModelError("file", "File is required and cannot be empty.");
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid file upload",
                    Detail = "The uploaded file is either missing or empty."
                };

                context.Result = new BadRequestObjectResult(problemDetails);
                return;
            }

            string hash;
            using var fileDataStream = fileItem.OpenReadStream();
            hash = HashHelper.ComputeSHA256Hash(fileDataStream);
            // reset stream position, otherwise it will affect subsequent file read operations
            if (fileDataStream.CanSeek)
            {
                fileDataStream.Position = 0;
            }
            context.HttpContext.Items.Add(Constants.Context_FileHash, hash);
            var existingFile = await _fileUploadService.GetUploadedItemAsync(fileItem.Length, hash);
            if (existingFile != null)
            {
                context.HttpContext.Items.Add(Constants.Context_ExistingFile, existingFile);

                //var fileExistsProblem = new ProblemDetails
                //{
                //    Status = StatusCodes.Status409Conflict,
                //    Title = "File already exists",
                //    Detail = $"A file with the same content (SHA256: {hash}) already exists in the system.",
                //    Instance = context.HttpContext.Request.Path
                //};
                //fileExistsProblem.Extensions.Add("existingFile", existingFile);
                //fileExistsProblem.Extensions.Add("fileHash", hash);

                //context.Result = new ObjectResult(fileExistsProblem)
                //{
                //    StatusCode = StatusCodes.Status409Conflict
                //};
                //return;
            }

            await next();
        } 
    }
}
