# Startup sample

This sample shows how you can leverage general dotnet concepts for performing initialization work in functions.

## Warmup Trigger?

This differs from a warmup trigger in that it does not rely on a functions feature to perform the work. It relies
on how dotnet hosted services work to either perform initialize before hosted services run, or in parallel with them.

## How does functions dotnet isolated startup work?

Dotnet isolated startup works like any other dotnet project: through `IHostedService` implementation(s). The function's
SDK registers an `IHostedService` which will establish a gRPC channel back to the host and post a message to it saying
it is ready. Since this all happens has part of running the `IHost` we can leverage all the initialization hooks any other
dotnet app offers.

### 1. Eager Initialization

For eager initialization we can simply _build_ the host, perform custom initialization, **then** run the host. Or alternatively,
initialization can occur as part of building the host. It all depends on how clever you are with the host lifecycle.

### 2. Lazy Initialization

There are many ways to do lazy initialization, but the question will be: when do you want to _start_ the lazy initialization?
The sample here starts it in parallel with the functions hosted service, by using a hosted service itself!
