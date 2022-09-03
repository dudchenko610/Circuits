namespace Circuits.ViewModels.Rendering;

public class SchemeRendererContext
{
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

    public Action OnUpdate { get; set; } = null!;
}