// "Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the MIT. See LICENSE file in the project root for full license information."

namespace Samples.Extensions.Rpc.Worker;

public interface IGreeter
{
    Task<string> SayHelloAsync(string name, CancellationToken cancellation = default);
}
