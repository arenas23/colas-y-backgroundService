
using Domain.Entities.Dto;
using Domain.Entities.Request;
using Domain.Interfaces;


namespace Services
{
    public class SenderService: ISenderService
    {
        private readonly IMessageProcessor _messageProcessor;
        private Random _random;
        public SenderService(IMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
            _random = new Random();
            
        }


        public async Task SendMessageToQueue(PeajeRequest request)
        {
            
            for (var i = 0; i < 100; i++) 
            {
                var message = new TransactionMessageDto
                {
                    Payload = request,
                    FirtAttemptTime = DateTime.UtcNow,
                    RetryCount = 0
                };
                await _messageProcessor.Publish(message);
            }
            

            Console.WriteLine("enviado");
        }

        public void ChangeConcurrency()
        {
            
        }
    }
}
