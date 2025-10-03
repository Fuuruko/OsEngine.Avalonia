using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace OsEngine.Models.Entity;

// internal class PositionJsonConverter : JsonConverter
// {
//     public override bool CanConvert(Type objectType)
//     {
//         return typeof(Position).IsAssignableFrom(objectType);
//     }
//
//     public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//     {
//         var settings = new JsonSerializerSettings
//         {
//             ContractResolver = new RequiredPropertiesContractResolver()
//         };
//         var json = JsonConvert.SerializeObject(value, settings);
//         writer.WriteRawValue(json);
//     }
//
//     public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//     {
//         // Implement deserialization if needed
//         return serializer.Deserialize(reader, objectType);
//     }
// }

public class JsonRequiredConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true; // Apply to all types
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var properties = value.GetType().GetProperties()
            .Where(prop => prop.GetCustomAttribute<JsonRequiredAttribute>() != null);

        writer.WriteStartObject();
        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(value);
            writer.WritePropertyName(property.Name);
            serializer.Serialize(writer, propertyValue);
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Implement deserialization if needed
        throw new NotImplementedException();
    }
}

// public class RequiredOnlyContractResolver : DefaultContractResolver
// {
//     protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
//     {
//         // Get all properties
//         var properties = base.CreateProperties(type, memberSerialization);
//
//         // Filter properties to include only those marked with [JsonRequired]
//         return properties.Where(p => p.AttributeProvider.GetAttributes(typeof(JsonRequiredAttribute), true).Any()).ToList();
//     }
// }
