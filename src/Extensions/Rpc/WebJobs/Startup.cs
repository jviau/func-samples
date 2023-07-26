// Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the MIT. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Rpc;
using Microsoft.Azure.WebJobs.Hosting;
using Samples.Extensions.Rpc.WebJobs;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Samples.Extensions.Rpc.WebJobs;

public sealed class Startup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddExtension<Extension>().MapWorkerGrpcService<GreeterImpl>();
    }
}
