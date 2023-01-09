using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures.Properties;

namespace Circuits.ViewModels.Entities.Structures;

public class Branch
{
    public List<Element> Elements { get; set; } = new();
    public Node NodeLeft { get; set; } = null!;
    public Node NodeRight { get; set; } = null!;

    public Resistance Resistance { get; set; } = null!;
    public Capacity Capacity { get; set; } = null!;
    public Inductance Inductance { get; set; } = null!;

    public ExpressionVariable Current { get; set; } = null!;
    public ExpressionDerivative CurrentDerivative { get; set; } = null!;
    public ExpressionVariable CapacityVoltage { get; set; } = null!;
    public ExpressionDerivative CapacityVoltageFirstDerivative { get; set; } = null!;
    public ExpressionDerivative CapacityVoltageSecondDerivative { get; set; } = null!;
    public List<Expression> DcVariables { get; set; } = new();
    public List<Expression> NonlinearElements { get; set; } = new();
}