// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Azure.Functions.Sdk;

public class WebJobsExtensions(List<WebJobsReference> extensions)
{
    public List<WebJobsReference> Extensions => extensions;
}
