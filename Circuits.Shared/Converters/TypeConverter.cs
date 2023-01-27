using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Circuits.Shared.Converters;

public class TypeConverter<TBaseItem> : JsonConverter<TBaseItem> where TBaseItem : class
{
    private readonly Dictionary<string, Type> _dictionary = new Dictionary<string, Type>();

    public TypeConverter()
    {
        foreach (var item in from myType in Assembly.GetAssembly(typeof(TBaseItem))!.GetTypes()
                 where myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(TBaseItem))
                 select myType)
        {
            _dictionary[item.Name] = item;
        }
    }

    private Type? GetTypeByCommandType(string commandType)
    {
        return _dictionary.TryGetValue(commandType, out var value) ? value : null;
    }

    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, TBaseItem? value, JsonSerializer serializer)
    {
        if (value is null) return; 
        
        var type = value.GetType();

        var resultObj = new JObject();
        var paramsObj = new JObject();

        resultObj.Add("Type", type.Name);

        foreach (var prop in type.GetProperties())
        {
            if (!prop.CanRead) continue;
            
            var propVal = prop.GetValue(value, null);
            if (propVal != null)
            {
                paramsObj.Add(prop.Name, JToken.FromObject(propVal, serializer));
            }
        }
        
        resultObj.Add("Properties", paramsObj);
        resultObj.WriteTo(writer);
    }

    public override TBaseItem? ReadJson(JsonReader reader, Type objectType, TBaseItem? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        var parameters = jsonObject.GetValue("Properties") as JObject;
        var jTokenType = jsonObject.GetValue("Type");

        if (parameters is null || !parameters.HasValues || jTokenType is null) return null;

        // Console.WriteLine($"Type: {type.Name}, \nProperties: {parameters}");

        var type = GetTypeByCommandType((string)(jTokenType as JValue)!.Value!);
        if (type is null) return null;
        
        var obj = (TBaseItem) JsonConvert.DeserializeObject("{}", type)!;
        
        serializer.Populate(parameters.CreateReader(), obj);
        
        return obj;
    }
}