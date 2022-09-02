namespace Circuits.ViewModels.Rendering;

public class SchemeRendererContext
{
    public bool PencilMode
    {
        get
        {
            OnUpdate?.Invoke();
            return _privateMode;
        }
        set => _privateMode = value;
    }

    private bool _privateMode = false;

    public Action OnUpdate { get; set; } = null!;
}