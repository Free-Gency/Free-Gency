using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FreeGency.Api.OpenApi;

/// <summary>
/// Enriches the generated OpenAPI document with the XML documentation comments
/// already written on the controllers, DTOs and services. The generator emits
/// the structural document; this transformer makes it human-readable by:
///  1. applying each action's <c>summary</c>/<c>remarks</c> to its operation,
///  2. attaching the <c>example</c>/<c>error example</c> JSON blocks found in the
///     action's remarks to the request body and the 200 response,
///  3. applying every type and property <c>summary</c> to its schema.
/// XML files are loaded once at startup from the output directory
/// (<c>FreeGency.Api.xml</c>, <c>FreeGency.AI.xml</c>, <c>FreeGency.Application.xml</c>).
/// </summary>
public sealed class XmlCommentsOpenApiTransformer : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private readonly IReadOnlyDictionary<string, XElement> _members;

    /// <summary>Creates the transformer and indexes the XML documentation files.</summary>
    public XmlCommentsOpenApiTransformer()
        => _members = LoadMembers();

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor action &&
            action.MethodInfo is { } method &&
            _members.TryGetValue(MethodDocId(method), out var member))
        {
            if (ElementText(member, "summary") is { Length: > 0 } summary)
                operation.Summary = summary;

            if (RemarksText(member) is { Length: > 0 } remarks)
                operation.Description = remarks;

            ApplyExamples(operation, member);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var jsonTypeInfo = context.JsonTypeInfo;
        var jsonProperty = context.JsonPropertyInfo;

        // For a property schema the CLR member is exposed through the attribute
        // provider; the type info on the context describes the property type,
        // not the declaring type. Only inline (non-$ref) schemas carry a
        // description here; referenced schemas are documented at the type level.
        if (jsonProperty is { AttributeProvider: PropertyInfo clrProperty } &&
            schema.Type is not null &&
            clrProperty.DeclaringType is { } declaring &&
            _members.TryGetValue($"P:{declaring.FullName}.{clrProperty.Name}", out var propertyMember))
        {
            if (ElementText(propertyMember, "summary") is { Length: > 0 } description)
                schema.Description = description;
        }
        else if (jsonTypeInfo?.Type is { } type &&
                 _members.TryGetValue($"T:{TypeDocId(type)}", out var typeMember))
        {
            if (ElementText(typeMember, "summary") is { Length: > 0 } description)
                schema.Description = description;
        }

        return Task.CompletedTask;
    }

    private static IReadOnlyDictionary<string, XElement> LoadMembers()
    {
        var map = new Dictionary<string, XElement>(StringComparer.Ordinal);

        foreach (var xmlFile in new[] { "FreeGency.Api.xml", "FreeGency.AI.xml", "FreeGency.Application.xml" })
        {
            var path = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (!File.Exists(path))
                continue;

            var document = XDocument.Load(path);
            if (document.Root?.Element("members") is not { } members)
                continue;

            foreach (var member in members.Elements("member"))
            {
                var name = member.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(name))
                    map.TryAdd(name, member);
            }
        }

        return map;
    }

    private void ApplyExamples(OpenApiOperation operation, XElement member)
    {
        foreach (var block in member.Descendants("code")
                     .Select(code => code.Value.Trim())
                     .Where(value => value.Length > 0))
        {
            if (TryParseJson(block) is not { } example)
                continue;

            // The request example carries the payload; the response examples are
            // told apart by their envelope: an error example has an "error"
            // member, the success example has "isSuccess": true and no error.
            // "error" is tested first because a validation error example can
            // also contain a "reviewText" key inside its validationErrors.
            if (block.Contains("\"error\"", StringComparison.Ordinal))
            {
                SetMediaTypeExample(ResponseContent(operation, "400"), example);
            }
            else if (block.Contains("\"isSuccess\"", StringComparison.Ordinal))
            {
                SetMediaTypeExample(ResponseContent(operation, "200"), example);
            }
            else if (block.Contains("\"reviewText\"", StringComparison.Ordinal))
            {
                SetMediaTypeExample(operation.RequestBody?.Content, example);
            }
        }
    }

    private static IDictionary<string, OpenApiMediaType>? ResponseContent(OpenApiOperation operation, string statusCode)
        => operation.Responses is { } responses && responses.TryGetValue(statusCode, out var response)
            ? response.Content
            : null;

    private static void SetMediaTypeExample(IDictionary<string, OpenApiMediaType>? content, JsonNode example)
    {
        if (content is null)
            return;

        if (!content.TryGetValue("application/json", out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            content["application/json"] = mediaType;
        }

        mediaType.Example = example;
    }

    private static JsonNode? TryParseJson(string json)
    {
        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string MethodDocId(MethodInfo method)
    {
        var parameters = string.Join(",", method.GetParameters().Select(parameter => TypeDocId(parameter.ParameterType)));
        return $"M:{method.DeclaringType!.FullName}.{method.Name}({parameters})";
    }

    private static string TypeDocId(Type type)
    {
        if (type.IsGenericType)
        {
            var definitionName = type.GetGenericTypeDefinition().FullName ?? type.Name;
            var backtick = definitionName.IndexOf('`');
            if (backtick > 0)
                definitionName = definitionName[..backtick];

            return definitionName + "{" + string.Join(",", type.GetGenericArguments().Select(TypeDocId)) + "}";
        }

        return type.FullName ?? type.Name;
    }

    private static string? ElementText(XElement member, string elementName)
    {
        var element = member.Element(elementName);
        return element is null ? null : Normalize(element.Value);
    }

    private static string? RemarksText(XElement member)
    {
        var remarks = member.Element("remarks");
        if (remarks is null)
            return null;

        var clone = new XElement(remarks);
        clone.Descendants("code").Remove();
        return Normalize(clone.Value);
    }

    private static string? Normalize(string value)
    {
        var joined = string.Join(
            " ",
            value.Replace("\r\n", "\n")
                 .Split('\n')
                 .Select(line => line.Trim())
                 .Where(line => line.Length > 0));

        return joined.Length == 0 ? null : joined;
    }
}
