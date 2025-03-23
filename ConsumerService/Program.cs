using Amazon;
using Amazon.SQS;
using ConsumerService.Services;
using Microsoft.Extensions.Hosting;
using ConsumerService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IAmazonSQS>(sp =>
        {
            return new AmazonSQSClient(RegionEndpoint.APSoutheast1);
        });

        services.AddHostedService<SqsConsumerService>();
    })
    .Build();

await host.RunAsync();