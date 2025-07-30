// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;

namespace TestProj;

public class Queue(IAzureClientFactory<ServiceBusSender> senders)
{
    [Function("queue-test")]
    public async Task Test([ServiceBusTrigger("test")] ServiceBusReceivedMessage message)
    {
        string body = message.Body.ToString();
        ServiceBusSender sender = senders.CreateClient("session-out");
        string correlationId = Guid.NewGuid().ToString();
        string sessionId = Guid.NewGuid().ToString();
        IEnumerable<ServiceBusMessage> messages = Enumerable.Range(0, 10).Select(i =>
            new ServiceBusMessage($"{i}") { CorrelationId = correlationId, SessionId = sessionId });
        await sender.SendMessagesAsync(messages);
    }
}
