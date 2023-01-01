using Circuits.ViewModels.Entities.Elements;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.ElementEdit;

public partial class ElementEditComponent
{
    [Parameter] public Element SelectedElement { get; set; } = null!;
}