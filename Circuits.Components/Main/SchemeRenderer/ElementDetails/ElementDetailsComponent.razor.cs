using Circuits.ViewModels.Entities.Elements;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer.ElementDetails;

public partial class ElementDetailsComponent
{
    [Parameter] public Element Element { get; set; } = null!;
}