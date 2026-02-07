using Microsoft.AspNetCore.Mvc;
using service.messaging.Clients.SignalR;
using service.shared.Models;
using Utils.Json;

namespace service.messaging.Controllers.SignalR
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SignalRMessagingController : ControllerBase
    {
        private readonly IRealtimeSender _realtimeSender;
        private readonly ILogger<SignalRMessagingController> _logger;

        public SignalRMessagingController(IRealtimeSender realtimeSender, ILogger<SignalRMessagingController> logger)
        {
            _realtimeSender = realtimeSender;
            _logger = logger;
        }

        /// <summary>
        /// Receives a request to send a real-time message to all connected clients via SignalR.
        /// </summary>
        /// <param name="message">message content</param>
        /// <returns>DoraemonMessage result</returns>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoraemonMessage>>> SendRealTimeMessage(
            [FromBody] DoraemonMessage message)
        {
            if (message == null || message.DoraemonItem == null)
            {
                _logger.LogWarning("DoraemonMessage is null.");
                return BadRequest(new { Message = "DoraemonMessage can not be null" });
            }

            try
            {
                await _realtimeSender.SendToGroupAsync(message.Topic, message);
                var result = ApiResponse<DoraemonMessage>.Success(message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var error = $"Failed to send message. [Topic: {message.Topic}, ItemId : {message.DoraemonItem.Id}]";
                _logger.LogError(ex, error);
                var result = ApiResponse<DoraemonMessage>.Fail(error, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }
    }
}
