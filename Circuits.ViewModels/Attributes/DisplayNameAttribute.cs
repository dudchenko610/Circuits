namespace Circuits.ViewModels.Attributes;

public class DisplayNameAttribute : Attribute
{
    public string Name { get; set; } = string.Empty;

    public DisplayNameAttribute(string name)
    {
        Name = name;
    }
}