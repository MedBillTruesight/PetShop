using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PetShop.Api.Swagger;

/// <summary>
/// Reorders Swagger UI tag groups so Customers and Orders appear first,
/// followed by ErrorTest, then Test. Populates root-level Tags from operations
/// (Swashbuckle often leaves it empty) and reorders Paths so tag first-occurrence
/// matches the desired order.
/// </summary>
public sealed class SwaggerTagOrderDocumentFilter : IDocumentFilter
{
    private static readonly string[] TagOrder = ["Customers", "Orders", "ErrorTest", "Test"];

    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        EnsureAndOrderTags(swaggerDoc);
        ReorderPaths(swaggerDoc);
    }

    private static void EnsureAndOrderTags(OpenApiDocument swaggerDoc)
    {
        var tagByName = new Dictionary<string, OpenApiTag>(StringComparer.OrdinalIgnoreCase);

        if (swaggerDoc.Tags != null)
        {
            foreach (var t in swaggerDoc.Tags)
                tagByName[t.Name] = t;
        }

        foreach (var (_, pathItem) in swaggerDoc.Paths)
        {
            foreach (var op in pathItem.Operations.Values)
            {
                if (op.Tags == null) continue;
                foreach (var t in op.Tags)
                {
                    if (t != null && !tagByName.ContainsKey(t.Name))
                        tagByName[t.Name] = new OpenApiTag { Name = t.Name, Description = t.Description };
                }
            }
        }

        var ordered = new List<OpenApiTag>();
        foreach (var name in TagOrder)
        {
            if (tagByName.TryGetValue(name, out var tag))
            {
                ordered.Add(tag);
                tagByName.Remove(name);
            }
        }
        foreach (var tag in tagByName.Values)
            ordered.Add(tag);

        swaggerDoc.Tags = ordered;
    }

    private static void ReorderPaths(OpenApiDocument swaggerDoc)
    {
        if (swaggerDoc.Paths == null || swaggerDoc.Paths.Count == 0)
            return;

        var pathOrder = new[] { "customers", "orders", "error-test", "test" };
        var sorted = swaggerDoc.Paths
            .OrderBy(kvp => PathGroupIndex(kvp.Key, pathOrder))
            .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
            .ToList();

        swaggerDoc.Paths.Clear();
        foreach (var (key, value) in sorted)
            swaggerDoc.Paths.Add(key, value);
    }

    private static int PathGroupIndex(string path, string[] pathOrder)
    {
        for (var i = 0; i < pathOrder.Length; i++)
        {
            if (path.Contains(pathOrder[i], StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return pathOrder.Length;
    }
}
