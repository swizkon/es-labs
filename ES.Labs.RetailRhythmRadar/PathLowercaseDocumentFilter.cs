using Microsoft.OpenApi.Models;
using RetailRhythmRadar.Configuration;
using RetailRhythmRadar.Domain.Commands;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RetailRhythmRadar;

public class PathLowercaseDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var dictionaryPath = swaggerDoc.Paths; //.ToDictionary(x => ToLowercase(x.Key), x => x.Value);
        var newPaths = new OpenApiPaths();

        foreach (var path in dictionaryPath)
        {
            //path.Value.Parameters = path.Value.Parameters.Select(parameter =>
            //{
            //    parameter.Name = parameter.Name.ToLowerInvariant() + "eller";
            //    return parameter;
            //}).ToList();

            //path.Value.Operations = path.Value.Operations.Select(x =>
            //{
            //    if (x.Key == OperationType.Post)
            //    {
            //        x.Value.RequestBody = new OpenApiRequestBody
            //        {
            //            Content = new Dictionary<string, OpenApiMediaType>
            //            {
            //                {
            //                    "application/json", new OpenApiMediaType
            //                    {
            //                        Schema = new OpenApiSchema
            //                        {
            //                            Type = "object",
            //                            Properties = new Dictionary<string, OpenApiSchema>
            //                            {
            //                                {
            //                                    "name", new OpenApiSchema
            //                                    {
            //                                        Type = "string"
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        };
            //    }
            //    return x;
            //}).ToDictionary();

            newPaths.Add(path.Key, path.Value);
        }


        foreach (var command in typeof(Setup).Assembly.GetTypes().Where(t =>
                     t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(ZoneCommand))))
        {
            swaggerDoc.Components.Schemas.Add(command.FullName!, new OpenApiSchema
            {
                Type = "object",
                Properties = command.GetProperties().Select(p =>
                {
                    var schema = new OpenApiSchema
                    {
                        Type = "string",
                        Nullable = false
                    };

                    return new KeyValuePair<string, OpenApiSchema>(p.Name, schema);
                }).ToDictionary()
            });

            var commandPath = new OpenApiPathItem
            {
                Summary = "End point to send a command",
                Description = "Trallalalsadlökl kaljk erjw rwer ewr for " + command.Name

            };
            commandPath.Operations.Add(OperationType.Post, new OpenApiOperation
            {
                Tags = new List<OpenApiTag>
                {
                    new()
                    {
                        Name = "Commands"
                    }
                },
                RequestBody = new OpenApiRequestBody
                {
                    Description = "Trallalalsadlökl kaljk erjw rwer ewr for " + command.Name,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.Schema,

                                        ExternalResource = $"#/components/schemas/{command.FullName}"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var path = "/commands/" + command.FullName;
            newPaths.Remove(path);
            newPaths.Add(path, commandPath);
        }

        swaggerDoc.Paths = newPaths;
    }

    private static string ToLowercase(string key)
    {
        var parts = key.Split('/').Select(part => part.Contains("}") ? part : part.ToLowerInvariant());
        return string.Join('/', parts);
    }
}