namespace Circuits.ViewModels.Rendering.Scheme;

public class SchemeRendererContext
{
    public const int CellSize = 30;
    public bool PencilMode
    {
        get => _privateMode;
        set
        {
            _privateMode = value;
            OnUpdate?.Invoke(); 
        }
    }

    private bool _privateMode = false;

    public Action? OnUpdate { get; set; } = null!;
}