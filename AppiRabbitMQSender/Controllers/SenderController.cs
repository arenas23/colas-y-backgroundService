
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

        [HttpGet]
        public async Task<IActionResult> parar()
        {
            try
            {
                await _service.ChangeConcurrency();
                return Ok("servicio detenido");
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("prender")]
        public async Task<IActionResult> Encender()
        {
            try
            {
                await _service.TurnOn();
                return Ok("se prendio esto");
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }

    }
}
