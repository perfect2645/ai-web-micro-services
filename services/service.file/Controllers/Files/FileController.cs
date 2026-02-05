using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using repository.doraemon.Repositories.Entities;
using service.file.Filters.Files;
using service.file.Services;
using System.ComponentModel.DataAnnotations;
using Utils.Json;

namespace service.file.Controllers.Files
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
    public class FileController(ILogger<FileController> logger, IFileUploadService fileUploadService) : ControllerBase
    {
        private readonly ILogger<FileController> _logger = logger;
        private readonly IFileUploadService _fileUploadService = fileUploadService;

        [HttpGet]
        [TypeFilter(typeof(FileKeyValidationFilterAttribute))]
        public async Task<ActionResult<ApiResponse<UploadedItem?>>> Get([FromQuery, Required] long fileSize,
            [FromQuery, Required] string sha256Hash)
        {
            var file = await _fileUploadService.GetUploadedItemAsync(fileSize, sha256Hash);
            var result = ApiResponse<UploadedItem?>.Success(file);

            return Ok(result);
        }

        [HttpPost]
        [TypeFilter(typeof(FileUploadValidationFilterAttribute))]
        public async Task<ActionResult<UploadedItem>> Upload(
            IFormFile file,
            [FromForm] string? description = null)
        {
            var fileRemoteUrl = await _fileUploadService.UploadFileAsync(file, description);
            return Ok(fileRemoteUrl);
        }
    }
}
