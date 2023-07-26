// "Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the MIT. See LICENSE file in the project root for full license information."

using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Samples.Extensions.Rpc.WebJobs;

internal class GreeterImpl : Greeter.GreeterBase
{
    private readonly ILogger _logger;

    public GreeterImpl(ILogger<GreeterImpl> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("SayHello called: {HelloRequestName}", request.Name);
        return Task.FromResult(new HelloReply() { Message = $"Hello, {request.Name}" });
    }
}
