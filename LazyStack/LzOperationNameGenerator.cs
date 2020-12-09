using System;
using System.Collections.Generic;
using System.Text;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace LazyStack
{
    public class LzOperationNameGenerator : IOperationNameGenerator
    {
        public bool SupportsMultipleClients { get; } = true;

        public string GetClientName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            return string.Empty;
        }

        public string GetOperationName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            return SolutionModel.RouteToEventName(httpMethod, path, operation.OperationId);
        }
    }
}
