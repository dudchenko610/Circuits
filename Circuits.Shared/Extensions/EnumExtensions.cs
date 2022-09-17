using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Circuits.Shared.Extensions;

public static class EnumExtensions
{
    public static R GetValue<R, T>(this Enum value, Func<T, R> predicate) where T : Attribute
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttributes = fieldInfo!.GetCustomAttributes(typeof(T), false) as T[];
        
        // if (descriptionAttributes[0].ResourceType != null)
        // {
        //     return OnLookupResource(descriptionAttributes[0].ResourceType!, predicate(descriptionAttributes[0]));
        // }

        return (descriptionAttributes is not null && descriptionAttributes.Length > 0) ? predicate(descriptionAttributes[0]) : default!;
    }

    // private static string OnLookupResource(IReflect resourceManagerProvider, string resourceKey)
    // {
    //     var resourceKeyProperty = resourceManagerProvider.GetProperty(resourceKey,
    //         BindingFlags.Static | BindingFlags.Public, null, typeof(string),
    //         types: Type.EmptyTypes, null);
    //
    //     if (resourceKeyProperty != null)
    //     {
    //         return (string) resourceKeyProperty.GetMethod!.Invoke(null, null)!;
    //     }
    //
    //     return resourceKey;
    // }
}