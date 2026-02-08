using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using repository.doraemon.Entities;
using repository.doraemon.Repositories.Entities;
using service.domain.Models;
using service.domain.Services;
using Utils.Json;

namespace service.domain.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
    public class DoraemonController(ILogger<DoraemonController> logger, IImageDataService imageDataService) : ControllerBase
    {
        private readonly ILogger<DoraemonController> _logger = logger;
        private readonly IImageDataService _imageDataService = imageDataService;


        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<List<DoraemonItem>?>>> GetByUserId(string userId)
        {
            var items = await _imageDataService.GetByUserIdAsync(userId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<UploadedItem>> Add([FromForm] DoraemonItemCreateDto createDto)
        {
            var result = await _imageDataService.AddAsync(createDto);
            return Ok(result);
        }

        [HttpPost("RabbitMq")]
        public async Task<ActionResult> SendMqMessage([FromBody] DoraemonItem doraemonItem)
        {
            try
            {
                await _imageDataService.PublishMqAsync(doraemonItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to MQ for DoraemonItem with Id: {Id}", doraemonItem.Id);
                return StatusCode(500, "Failed to publish message to MQ.");
            }
            return Ok();
        }
    }
}
