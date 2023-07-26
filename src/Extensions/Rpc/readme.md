# Extensions RPC sample

This sample shows how a dotnet worker extension can leverage gRPC to perform custom communication between the worker and host extension. In this sample, there is a simple WebJobs extension which has a gRPC greeter service and a dotnet worker extension which has a client that can talk to the WebJobs service.

## What is this for?

Functions RPC extensibility is to solve cases where a worker extension needs custom communication with its respective WebJobs extension on the host side. This can be used for many advanced scenarios:

1. Custom worker side type binding, where the bound type interacts back to the WebJobs extension via gRPC.
    - eg: Service Bus SDK types hold locks which allow or deny interaction with a message from the queue. Since the ServiceBus WebJobs extension is the one that technically holds this lock, the ServiceBus worker extension can provide bindable types which interact back to that lock owner via gRPC.
2. Adding custom services available to worker which interact back to the WebJobs extension via gRPC.
    - eg: Durable Functions has a client which needs to interact with a similar client in the WebJobs extension. This can be accomplished by RPC to that WebJobs client.

## Grpc Toolchain

RPC extension communication relies on the existing gRPC tool chain. See [Grpc.Tools](https://www.nuget.org/packages/Grpc.Tools/#readme-body-tab) for more information on how to use gRPC in .NET. Functions RPC extensibility does not have any special requirements or restrictions on how to author a gRPC proto. There are plenty of online resources on how to use gRPC in .NET.

## WebJobs Extension

By adding a reference to `Microsoft.Azure.WebJobs.Extensions.Rpc` we get access to the `IWebJobsExtensionBuilder.MapWorkerGrpcService` extension method. This method will register a gRPC service to the functions gRPC host, allowing it to be called by the worker.

The WebJobs sample only shows how to register a gRPC service. It does not show how this service would integrate with an extension as that is to be decided on a per-implementation basis.

## Worker Extension

On the worker side we only need to register the gRPC client with the appropriate `CallInvoker` so that it will call the service registered in the previous section. Doing this is as simple as importing `IOptions<FunctionsGrpcOptions>` and using the `FunctionsGrpcOptions.CallInvoker` property. All gRPC clients built via `Grpc.Tools` should have a constructor which accepts a `CallInvoker`.

The worker extension sample only shows getting the gRPC client configured and in the service container, allowing for usage via dependency injection as needed. How exactly a gRPC client fits into a functions extension is to be decided by that respective extension.
