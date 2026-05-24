using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskFlow.IntegrationTests.Infrastructure;

internal static class TestJsonOptions
{
    internal static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
