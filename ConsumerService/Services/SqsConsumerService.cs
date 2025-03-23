using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using ConsumerService.Models;
using System.Net.Http.Json;

namespace ConsumerService.Services
{
    public class SqsConsumerService : BackgroundService
    {
        private readonly ILogger<SqsConsumerService> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;
        private readonly IHttpClientFactory _httpClientFactory;

        public SqsConsumerService(ILogger<SqsConsumerService> logger, 
            IAmazonSQS sqsClient, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory) 
        {
            _logger = logger;
            _sqsClient = sqsClient;
            _httpClientFactory = httpClientFactory;

            var queueUrl = configuration["SqsQueueUrl"];
            if (string.IsNullOrWhiteSpace(queueUrl))
            {
                throw new Exception("Queue URL is missing from configuration!");
            }

            _queueUrl = queueUrl;  
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
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
                        var order = JsonSerializer.Deserialize<OrderDto>(message.Body);

                        if (order is not null)
                        {
                            _logger.LogInformation("Received Order: {OrderId} - {Product} ", order.OrderId, order.ProductName);

                            var client = _httpClientFactory.CreateClient();
                            var callbackResponse = await client.PostAsJsonAsync("https://localhost:44394/api/v1/orders/callback", order, stoppingToken);

                            _logger.LogInformation("Call API Callback : {StatusCode}", callbackResponse.StatusCode);
                        }
                        else
                        {
                            _logger.LogWarning("Can't read OrderDto !!");
                        }


                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                        _logger.LogInformation("Delete message.");
                    }

                    await Task.Delay(2000, stoppingToken);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Exception !!");
            }
        }
    }
}
