using Circuits.ViewModels.Entities.Elements;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.ElementOptions;

public partial class ElementOptionsComponent
{
    [Parameter] public Element SelectedElement { get; set; } = null!;
}