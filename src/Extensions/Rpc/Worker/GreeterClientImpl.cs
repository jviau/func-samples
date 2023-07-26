// "Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the MIT. See LICENSE file in the project root for full license information."

using Microsoft.Extensions.Logging;

namespace Samples.Extensions.Rpc.Worker;

internal sealed class GreeterImpl : IGreeter
{
    private readonly Greeter.GreeterClient _client;
    private readonly ILogger _logger;

    public GreeterImpl(Greeter.GreeterClient client, ILogger<GreeterImpl> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> SayHelloAsync(string name, CancellationToken cancellation = default)
    {
        _logger.LogInformation("Saying hello from {GreeterName}.", name);
        HelloReply reply = await _client.SayHelloAsync(new() { Name = name }, cancellationToken: cancellation);
        return reply.Message;
    }
}
