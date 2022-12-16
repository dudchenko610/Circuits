using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Circuits.Components.Common.NumberInput;

public class BCHNumberInput<TValue> : ComponentBase
{
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }
    [Parameter] public Expression<Func<TValue>>? ValueExpression { get; set; }
    [Parameter] public string? DisplayName { get; set; }
    [Parameter] public bool CssClass { get; set; }

    private TValue? CurrentValue
    {
        get => Value;
        set
        {
            var hasChanged = !EqualityComparer<TValue>.Default.Equals(value, Value);
            if (hasChanged)
            {
                Value = value;
                _ = ValueChanged.InvokeAsync(Value);
            }
        }
    }
    
    private string? CurrentValueAsString
    {
        get => CurrentValue switch
        {
            null => null,
            int @int => BindConverter.FormatValue(@int, CultureInfo.InvariantCulture),
            long @long => BindConverter.FormatValue(@long, CultureInfo.InvariantCulture),
            short @short => BindConverter.FormatValue(@short, CultureInfo.InvariantCulture),
            float @float => BindConverter.FormatValue(@float, CultureInfo.InvariantCulture),
            double @double => BindConverter.FormatValue(@double, CultureInfo.InvariantCulture),
            decimal @decimal => BindConverter.FormatValue(@decimal, CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException($"Unsupported type {CurrentValue.GetType()}")
        };
        set
        {
            if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.InvariantCulture, out var result))
            {
                CurrentValue = result!;
            }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "step", "any");
        builder.AddAttribute(2, "type", "number");
        builder.AddAttribute(3, "class", CssClass);
        builder.AddAttribute(4, "lang", "en-150");
        builder.AddAttribute(5, "value", BindConverter.FormatValue(CurrentValueAsString));
        builder.AddAttribute(6, "onchange",
            EventCallback.Factory.CreateBinder<string?>(this, value => CurrentValueAsString = value,
                CurrentValueAsString));
        builder.CloseElement();
    }
}