namespace Circuits.ViewModels.Events;

public class ScrollEventArgs : EventArgs
{
    public double ScrollTop { get; set; }
    public double ScrollLeft { get; set; }
    public double ScrollHeight { get; set; }
    public double ClientWidth { get; set; }
    public double ClientHeight { get; set; }

}
