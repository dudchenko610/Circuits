using Circuits.ViewModels.Entities.Elements;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer.Elements;

public class ElementComponent : ComponentBase
{
    [CascadingParameter(Name="SchemeRenderReference")] public SchemeRendererComponent SchemeRenderer { get; set; } = null!;
    [Parameter] public RenderFragment<Element> Template { get; set; } = null!;
    [Parameter] public Type Key { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        //Console.WriteLine($"OnInitialized");

        SchemeRenderer.AddElement(this);
    }
}