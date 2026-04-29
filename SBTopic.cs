using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DemoFunctionApp;

public class SBTopic
{
    private readonly ILogger<SBTopic> _logger;

    public SBTopic(ILogger<SBTopic> logger)
    {
        _logger = logger;
    }

    [Function(nameof(SBTopic))]
    public async Task Run(
        [ServiceBusTrigger("saurabhtopics", "saurabhSub", Connection = "saurabhSAS")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}