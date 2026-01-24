using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PetShop.Api.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PetShop.Api.Swagger;

/// <summary>
/// Adds request, response, and error examples to Swagger documentation.
/// </summary>
public sealed class SwaggerExampleOperationFilter : IOperationFilter
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var method = context.ApiDescription.HttpMethod ?? string.Empty;

        AddRequestBodyExamples(operation, path, method);
        AddSuccessResponseExamples(operation, path, method);
        AddErrorResponseExamples(operation, context);
    }

    private static void AddRequestBodyExamples(OpenApiOperation operation, string path, string method)
    {
        if (operation.RequestBody?.Content?.ContainsKey("application/json") != true)
            return;

        var content = operation.RequestBody.Content["application/json"];
        IOpenApiAny? example = (path, method) switch
        {
            ("api/v{version}/customers", "POST") => CreateCreateCustomerRequestExample(),
            ("api/v{version}/customers/{id}", "PUT") => CreateUpdateCustomerRequestExample(),
            ("api/v{version}/orders", "POST") => CreateCreateOrderRequestExample(),
            ("api/v{version}/orders/{id}", "PATCH") => CreateUpdateOrderRequestExample(),
            ("api/v{version}/orders/{id}/transition", "POST") => CreateTransitionOrderRequestExample(),
            ("api/v{version}/orders/{id}/pets", "POST") => CreateAddPetRequestExample(),
            _ => null
        };

        if (example != null)
            content.Example = example;
    }

    private static void AddSuccessResponseExamples(OpenApiOperation operation, string path, string method)
    {
        switch ((path, method))
        {
            case ("api/v{version}/customers", "POST"):
                AddExampleToResponse(operation, "201", CreateCustomerResponseExample());
                break;
            case ("api/v{version}/customers", "GET"):
                AddExampleToResponse(operation, "200", CreateCustomersListResponseExample());
                break;
            case ("api/v{version}/customers/{id}", "GET"):
            case ("api/v{version}/customers/{id}", "PUT"):
                AddExampleToResponse(operation, "200", CreateCustomerResponseExample());
                break;
            case ("api/v{version}/orders", "GET"):
                AddExampleToResponse(operation, "200", CreateOrdersListResponseExample());
                break;
            case ("api/v{version}/orders", "POST"):
                AddExampleToResponse(operation, "201", CreateOrderResponseExample());
                break;
            case ("api/v{version}/orders/{id}", "GET"):
            case ("api/v{version}/orders/{id}", "PATCH"):
            case ("api/v{version}/orders/{id}/transition", "POST"):
                AddExampleToResponse(operation, "200", CreateOrderResponseExample());
                break;
            case ("api/v{version}/customers/{id}/orders", "GET"):
                AddExampleToResponse(operation, "200", CreateCustomerOrdersResponseExample());
                break;
            default:
                if (path.StartsWith("api/v{version}/orders/", StringComparison.Ordinal) && path.EndsWith("/pets", StringComparison.Ordinal) && method == "POST")
                    AddExampleToResponse(operation, "201", CreateOrderWithPetResponseExample());
                break;
        }
    }

    private static void AddErrorResponseExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var errorSchema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository);

        foreach (var (status, code, message) in new[] {
            ("400", "VALIDATION_ERROR", "Required parameter 'request' cannot be null."),
            ("404", "ORDER_NOT_FOUND", "Order with ID {id} was not found."),
            ("409", "ORDER_INVALID_STATE", "Cannot add pets to order in Processing status. Pets can only be added when order is Open."),
            ("422", "INVALID_PICKUP_DATE", "Pickup date must be today or in the future."),
        })
        {
            if (!operation.Responses.TryGetValue(status, out var response))
                continue;

            response.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!response.Content.ContainsKey("application/json"))
                response.Content["application/json"] = new OpenApiMediaType();

            var mediaType = response.Content["application/json"];
            mediaType.Schema ??= errorSchema;
            if (mediaType.Example == null)
                mediaType.Example = CreateErrorExample(code, message);
        }
    }

    private static void AddExampleToResponse(OpenApiOperation operation, string status, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(status, out var response))
            return;
        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        if (!response.Content.ContainsKey("application/json"))
            return;
        var mediaType = response.Content["application/json"];
        if (mediaType.Example == null)
            mediaType.Example = example;
    }

    private static OpenApiObject CreateCreateCustomerRequestExample() => new()
    {
        ["firstName"] = new OpenApiString("Jane"),
        ["lastName"] = new OpenApiString("Doe"),
        ["email"] = new OpenApiString("jane.doe@example.com"),
        ["phone"] = new OpenApiString("555-1234")
    };

    private static OpenApiObject CreateUpdateCustomerRequestExample() => new()
    {
        ["firstName"] = new OpenApiString("Jane"),
        ["lastName"] = new OpenApiString("Smith"),
        ["email"] = new OpenApiString("jane.smith@example.com"),
        ["phone"] = new OpenApiString("555-5678")
    };

    private static OpenApiObject CreateCreateOrderRequestExample() => new()
    {
        ["customerId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["pickupDate"] = new OpenApiString(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString(DateFormat))
    };

    private static OpenApiObject CreateUpdateOrderRequestExample() => new()
    {
        ["pickupDate"] = new OpenApiString(DateOnly.FromDateTime(DateTime.Today.AddDays(2)).ToString(DateFormat))
    };

    private static OpenApiObject CreateTransitionOrderRequestExample() => new()
    {
        ["targetStatus"] = new OpenApiInteger(1) // Processing
    };

    private static OpenApiObject CreateAddPetRequestExample() => new()
    {
        ["name"] = new OpenApiString("Fluffy"),
        ["price"] = new OpenApiDouble(100.50),
        ["kind"] = new OpenApiString("Cat"),
        ["color"] = new OpenApiString("White")
    };

    private static OpenApiObject CreateErrorExample(string code, string message) => new()
    {
        ["error"] = new OpenApiObject
        {
            ["code"] = new OpenApiString(code),
            ["message"] = new OpenApiString(message)
        }
    };

    private static OpenApiObject CreateCustomerResponseExample() => new()
    {
        ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["firstName"] = new OpenApiString("Jane"),
        ["lastName"] = new OpenApiString("Doe"),
        ["email"] = new OpenApiString("jane.doe@example.com"),
        ["phone"] = new OpenApiString("555-1234"),
        ["estimatedPaymentDue"] = new OpenApiDouble(0),
        ["actualPaymentDue"] = new OpenApiDouble(0)
    };

    private static OpenApiObject CreateOrderResponseExample() => new()
    {
        ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["customerId"] = new OpenApiString("7a85f64-5717-4562-b3fc-2c963f66afa7"),
        ["status"] = new OpenApiInteger(0),
        ["pickupDate"] = new OpenApiString(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString(DateFormat)),
        ["pets"] = new OpenApiArray(),
        ["estimatedCost"] = new OpenApiDouble(0)
    };

    private static OpenApiObject CreateOrderWithPetResponseExample() => new()
    {
        ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["customerId"] = new OpenApiString("7a85f64-5717-4562-b3fc-2c963f66afa7"),
        ["status"] = new OpenApiInteger(0),
        ["pickupDate"] = new OpenApiString(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString(DateFormat)),
        ["pets"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["id"] = new OpenApiString("8b96f64-5717-4562-b3fc-2c963f66afa8"),
                ["name"] = new OpenApiString("Fluffy"),
                ["price"] = new OpenApiDouble(100.50),
                ["kind"] = new OpenApiString("Cat"),
                ["color"] = new OpenApiString("White")
            }
        },
        ["estimatedCost"] = new OpenApiDouble(100.50)
    };

    private static OpenApiArray CreateCustomersListResponseExample() => new()
    {
        CreateCustomerResponseExample(),
        new OpenApiObject
        {
            ["id"] = new OpenApiString("7b85f64-5717-4562-b3fc-2c963f66afa7"),
            ["firstName"] = new OpenApiString("Jane"),
            ["lastName"] = new OpenApiString("Smith"),
            ["email"] = new OpenApiString("jane.smith@example.com"),
            ["phone"] = new OpenApiString("555-9999"),
            ["estimatedPaymentDue"] = new OpenApiDouble(150.00),
            ["actualPaymentDue"] = new OpenApiDouble(250.00)
        }
    };

    private static OpenApiArray CreateOrdersListResponseExample() => new()
    {
        CreateOrderResponseExample(),
        new OpenApiObject
        {
            ["id"] = new OpenApiString("7b85f64-5717-4562-b3fc-2c963f66afa7"),
            ["customerId"] = new OpenApiString("8c85f64-5717-4562-b3fc-2c963f66afa8"),
            ["status"] = new OpenApiInteger(2),
            ["pickupDate"] = new OpenApiString(DateOnly.FromDateTime(DateTime.Today.AddDays(2)).ToString(DateFormat)),
            ["pets"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("9d96f64-5717-4562-b3fc-2c963f66afa9"),
                    ["name"] = new OpenApiString("Buddy"),
                    ["price"] = new OpenApiDouble(150.00),
                    ["kind"] = new OpenApiString("Dog"),
                    ["color"] = new OpenApiString("Brown")
                }
            },
            ["actualCost"] = new OpenApiDouble(150.00)
        }
    };

    private static OpenApiArray CreateCustomerOrdersResponseExample() => new()
    {
        CreateOrderResponseExample(),
        new OpenApiObject
        {
            ["id"] = new OpenApiString("7b85f64-5717-4562-b3fc-2c963f66afa7"),
            ["customerId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            ["status"] = new OpenApiInteger(2),
            ["pickupDate"] = new OpenApiString(DateOnly.FromDateTime(DateTime.Today.AddDays(3)).ToString(DateFormat)),
            ["pets"] = new OpenApiArray(),
            ["actualCost"] = new OpenApiDouble(0.00)
        }
    };
}
