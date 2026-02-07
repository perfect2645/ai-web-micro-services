using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using service.messaging.Services;
using service.shared.Models;

namespace WebapiMq.Controllers.RabbitMq
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
    public class RabbitMqMessagingController : ControllerBase
    {
        private readonly ILogger<RabbitMqMessagingController> _logger;
        private IDoraemonMessageService _messageService;

        public RabbitMqMessagingController(ILogger<RabbitMqMessagingController> logger, IDoraemonMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;
        }

        [HttpPost]
        [Route("DoraemonMessage")]
        public async Task<IActionResult> SendMessage([FromBody] DoraemonMessage message)
        {
            await _messageService.SendImageMessageAsync(message);
            return Ok();
        }

        [HttpPost]
        [Route("DoraemonTopicMessage")]
        public async Task<IActionResult> SendTopicMessage([FromBody] DoraemonMessage message)
        {
            await _messageService.SendTopicMessageAsync(message.Topic, message);
            return Ok();
        }
    }
}
