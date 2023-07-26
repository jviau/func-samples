// "Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the MIT. See LICENSE file in the project root for full license information."

using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace Samples.Extensions.Rpc.WebJobs;

[Extension("RpcSample", "RpcSample")]
public sealed class Extension : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        // no op.
    }
}
