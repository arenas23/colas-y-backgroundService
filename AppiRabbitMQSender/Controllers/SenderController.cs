
using Domain.Entities.Request;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppiRabbitMQSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        private readonly ISenderService _service;
        public SenderController(ISenderService senderService) {
            _service = senderService;
        }
        // POST api/<SenderController>
        [HttpPost]
        public IActionResult Post([FromBody] PeajeRequest value)
        {
            try
            {
                _service.SendMessageToQueue(value);
                return Ok("mensaje enviado");
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }

    }
}
