namespace Circuits.ViewModels.Rendering;

public class SchemeRendererContext
{
    public bool PencilMode
    {
        get => _privateMode;
        set
        {
            OnUpdate?.Invoke(); 
            _privateMode = value;
        }
    }

    private bool _privateMode = false;

    public Action OnUpdate { get; set; } = null!;
}