
using Circuits.ViewModels.Attributes;

namespace Circuits.ViewModels.Entities.Equations;

public enum MathOperation
{
    [DisplayName("+")]
    Plus,
    [DisplayName("-")]
    Minus,
    [DisplayName("*")]
    Multiply,
    [DisplayName("/")]
    Divide
}