using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using elasticsearch.Attributes;
using elasticsearch.Models.Enumerations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace elasticsearch.Extensions.Json;

public static class SensitiveDataExtensions
{
    /// <summary>
    /// Método que verifica se propriedades do request possui o atributo <see cref="SensitiveDataAttribute"/> e mascara os dados
    /// </summary>
    /// <param name="requestObj"></param>
    /// <param name="requestType"></param>
    /// <returns></returns>
    public static string SanitizeSensitiveData(string requestObj, Type requestType)
    {
        try
        {
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestObj,
                new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });

            SanitizeProperties(jsonObject, requestType);

            return JsonConvert.SerializeObject(jsonObject);
        }
        catch
        {
            return requestObj;
        }
    }

    private static void SanitizeProperties(Dictionary<string, object> jsonObject, Type type)
    {
        foreach (var property in type.GetProperties())
        {
            // Verifico se o atributo SensitiveData está presente na propriedade
            if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)))
            {
                var attribute = (SensitiveDataAttribute)property.GetCustomAttribute(typeof(SensitiveDataAttribute))!;
                var sensitiveEnum = SensitiveEnum.FromValue((int)attribute.SensitivityLevel);

                if (sensitiveEnum is not null)
                {
                    string key = property.Name;
                    var matchingKey = jsonObject.Keys.FirstOrDefault(keyObject =>
                        string.Equals(keyObject, key, StringComparison.OrdinalIgnoreCase));

                    if (matchingKey != null && jsonObject[matchingKey] != null)
                    {
                        // Aplica a máscara se for uma string
                        if (jsonObject[matchingKey] is string value && !string.IsNullOrEmpty(value))
                        {
                            jsonObject[matchingKey] = sensitiveEnum.Mask(value);
                        }
                    }
                }
            }
            else
            {
                // Caso a propriedade nao tenha o atributo, refaço a checagem do match do nome da prop da classe com o jsonObject
                var matchingKey = jsonObject.Keys.FirstOrDefault(keyObject =>
                    string.Equals(keyObject, property.Name, StringComparison.OrdinalIgnoreCase));

                //Verifico se a prop do json é uma classe aninhada
                if (matchingKey != null && jsonObject[matchingKey] is JObject nestedObject)
                {
                    // Faz o recursivo pra mascarar as propriedades da classe aninhada
                    var nestedDictionary = nestedObject.ToObject<Dictionary<string, object>>();
                    SanitizeProperties(nestedDictionary, property.PropertyType);
                    jsonObject[matchingKey] = nestedDictionary;
                }
            }
        }
    }
}