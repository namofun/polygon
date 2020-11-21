using Microsoft.OpenApi.Models;
using SatelliteSite.PolygonModule.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace SatelliteSite.PolygonModule
{
    public class SwaggerFixFilter : IOperationFilter
    {
        delegate void FixApply(OpenApiOperation operation, OperationFilterContext context);

        private static readonly Dictionary<string, FixApply> _fixes = new Dictionary<string, FixApply>
        {
            ["SatelliteSite.PolygonModule.Apis.JudgehostsController.AddJudgingRun (SatelliteSite.PolygonModule)"] = (operation, context) =>
            {
                if (!context.SchemaRepository.TryGetIdFor(typeof(JudgingRunModel), out var schemaId) ||
                    !context.SchemaRepository.Schemas.TryGetValue(schemaId, out var schema))
                {
                    throw new InvalidOperationException("Unknown api structure.");
                }

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/x-www-form-urlencoded"] = new OpenApiMediaType
                        {
                            Schema = schema,
                        }
                    }
                };

                context.SchemaRepository.Schemas.Remove(schemaId);
            },
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var name = context.ApiDescription?.ActionDescriptor?.DisplayName ?? string.Empty;
            if (_fixes.TryGetValue(name, out var fixApply))
            {
                fixApply.Invoke(operation, context);
            }
        }
    }
}
