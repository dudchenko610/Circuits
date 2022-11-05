using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Circuits.Shared.Extensions;

public static class EnumExtensions
{
    public static R GetValue<R, T>(this Enum value, Func<T, R> predicate) where T : Attribute
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttributes = fieldInfo!.GetCustomAttributes(typeof(T), false) as T[];
        
        return (descriptionAttributes is not null && descriptionAttributes.Length > 0) ? predicate(descriptionAttributes[0]) : default!;
    }
}