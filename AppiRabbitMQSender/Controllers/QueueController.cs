
using Domain.Entities.Request;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppiRabbitMQSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _service;
        public QueueController(IQueueService senderService) {
            _service = senderService;
        }
        // POST api/<SenderController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PeajeRequest value)
        {
            try
            {
                await _service.SendMessageToQueue(value);
                return Ok("mensaje enviado");
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("stop")]
        public async Task<IActionResult> StopQueues()
        {
            try
            {
                await _service.StopConsumers();
                return Ok("Servicios detenidos");
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("start")]
        public async Task<IActionResult> Encender()
        {
            try
            {
                await _service.TurnOnConsumers();
                return Ok("Servicios arriba");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
