using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace elasticsearch.Extensions.Json;

public static class JsonHelper
{

    public static object DeserializeJson(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return null;

        using JsonDocument doc = JsonDocument.Parse(jsonString);
        return JsonHelper.TraverseJsonDocument(doc.RootElement);
    }
    public static Dictionary<string, string> DeserializeJson(IHeaderDictionary headerDictionary)
    {
        if (headerDictionary == null) return new Dictionary<string, string>();

        var relevantData = new Dictionary<string, string>();

        foreach (var kvp in headerDictionary)
        {
            relevantData[kvp.Key] = kvp.Value;
        }

        return relevantData;
    }
    public static string ConvertHeadersToJson(IHeaderDictionary headers)
    {
        Dictionary<string, string[]> headersDictionary = new Dictionary<string, string[]>();

        foreach (var header in headers)
        {
            headersDictionary.Add(header.Key, header.Value);
        }

        return JsonConvert.SerializeObject(headersDictionary, Formatting.Indented);
    }

    public static Dictionary<string, string[]> ConvertHeadersToDictionary(IHeaderDictionary headers)
    {
        var headersDictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, values) in headers)
        {
            headersDictionary[key] = values.ToArray();
        }

        return headersDictionary;
    }
    public static object TraverseJsonDocument(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    obj[property.Name] = TraverseJsonDocument(property.Value);
                }
                return obj;

            case JsonValueKind.Array:
                var array = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(TraverseJsonDocument(item));
                }
                return array;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                // You may want to handle different number types (int, float, etc.) based on your requirements
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return null;
        }
    }
}