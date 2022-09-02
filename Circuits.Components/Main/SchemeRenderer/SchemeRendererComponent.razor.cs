using System;
using Circuits.ViewModels.Events;
using Circuits.ViewModels.Rendering;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeRenderer;

class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

class Wire
{
    public List<Point> Points { get; set; } = new();
}

public partial class SchemeRendererComponent : IDisposable
{
    [Parameter] public SchemeRendererContext SchemeRendererContext { get; set; } = null!;
    [Parameter] public float Scale { get; set; } = 1.0f;

    protected override void OnInitialized()
    {
        SchemeRendererContext.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        SchemeRendererContext.OnUpdate -= StateHasChanged;
    }

    private void OnMouseMove(ExtMouseEventArgs e)
    {
        var schemeRendererContainer = e.PathCoordinates.FirstOrDefault(x => x.ClassList.Contains("scheme-renderer-container"));
        
        if (schemeRendererContainer is null)
            return;
        
        Console.WriteLine($"X: {schemeRendererContainer.X / Scale} Y: {schemeRendererContainer.Y / Scale}");
    }
}

