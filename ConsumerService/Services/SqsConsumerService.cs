using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerService.Services
{
    public class SqsConsumerService : BackgroundService
    {
        private readonly ILogger<SqsConsumerService> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public SqsConsumerService(ILogger<SqsConsumerService> logger, IAmazonSQS sqsClient, IConfiguration configuration) 
        {
            _logger = logger;
            _sqsClient = sqsClient;

            var queueUrl = configuration["SqsQueueUrl"];
            if (string.IsNullOrWhiteSpace(queueUrl))
            {
                throw new Exception("Queue URL is missing from configuration!");
            }

            _queueUrl = configuration["SqsQueueUrl"]!;  
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQS FIFO Consumer started. Listening to: {QueueUrl}" + _queueUrl);

            while (!stoppingToken.IsCancellationRequested) 
            {
                var requset = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 10,
                    AttributeNames = new List<string> { "All" },
                    MessageAttributeNames = new List<string> { "All" }
                };

                var response = await _sqsClient.ReceiveMessageAsync(requset, stoppingToken);

                foreach (var message in response.Messages) 
                {
                    _logger.LogInformation("Received: " + message.Body);

                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                    _logger.LogInformation("Delete message.");
                }

                await Task.Delay(2000, stoppingToken);
            }

            throw new NotImplementedException();
        }
    }
}
