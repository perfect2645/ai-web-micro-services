using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using repository.file.Repositories.Entities;
using service.file.Filters.Files;
using service.file.Services;
using System.ComponentModel.DataAnnotations;
using Utils.Json;

namespace service.file.Controllers.Files
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(1.0)]
    public class FileController : ControllerBase
    {

        private readonly ILogger<FileController> _logger;
        private readonly IFileUploadService _fileUploadService;

        public FileController(ILogger<FileController> logger, IFileUploadService fileUploadService)
        {
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

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
        public async Task<ActionResult<Uri>> Upload(
            [FromForm] IFormFile file,
            [FromForm] string? description = null)
        {
            var fileRemoteUrl = await _fileUploadService.UploadFileAsync(file, description);
            return Ok(fileRemoteUrl);
        }
    }
}
