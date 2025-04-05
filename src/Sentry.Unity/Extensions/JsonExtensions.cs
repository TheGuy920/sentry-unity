using System;
using Newtonsoft.Json.Linq;

namespace Sentry.Unity.Extensions;

internal static class JsonExtensions
{
    // Converted from System.Text.Json to Newtonsoft.Json
    public static JToken? GetPropertyOrNull(this JToken json, string name)
    {
        if (json.Type != JTokenType.Object)
        {
            return null;
        }

        var property = json[name];
        if (property == null || property.Type == JTokenType.Null || property.Type == JTokenType.Undefined)
        {
            return null;
        }

        return property;
    }

    public static TEnum? GetEnumOrNull<TEnum>(this JToken json, string name)
        where TEnum : struct
    {
        var property = json.GetPropertyOrNull(name);
        var enumString = property?.ToString();
        if (string.IsNullOrWhiteSpace(enumString))
        {
            return null;
        }

        if (!Enum.TryParse(enumString, true, out TEnum value))
        {
            return null;
        }

        return value;
    }
}
