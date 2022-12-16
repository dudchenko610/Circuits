using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Circuits.Components.Common.Input;

public partial class BCHInput<T> : InputNumber<T>
{
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    
    private string stringValue;
    private T lastParsedValue;

    protected override void OnParametersSet()
    {
        // Only overwrite the "stringValue" when the Value is different
        if (Equals(CurrentValue, lastParsedValue)) return;
        
        lastParsedValue = CurrentValue;
        stringValue = CurrentValueAsString;
    }

    private void OnInput(ChangeEventArgs e)
    {
        // Update the value
        CurrentValueAsString = stringValue = (string)e.Value;
        lastParsedValue = CurrentValue;
    }

    private void OnBlur(FocusEventArgs e)
    {
        // Overwrite the stringValue property with the parsed value.
        // This call Value.ToString(), so the value in the input is well formatted.
        // note: Ensure the string value is valid before updating the content
        if (!EditContext.GetValidationMessages(FieldIdentifier).Any())
        {
            stringValue = CurrentValueAsString;
        }
    }
}