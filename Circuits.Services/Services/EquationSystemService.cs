using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Math;

namespace Circuits.Services.Services;

public class EquationSystemService : IEquationSystemService
{
    private readonly ISchemeService _schemeService;

    public EquationSystemService(ISchemeService schemeService)
    {
        _schemeService = schemeService;
    }

    public List<EquationSystem> BuildEquationSystemsFromGraphs(IEnumerable<Graph> graphs)
    {
        var equationSystems = new List<EquationSystem>();

        foreach (var branch in _schemeService.Branches)
        {
            branch.Current = null!;
            branch.CurrentDerivative = null!;
            branch.CapacityVoltage = null!;
            branch.CapacityVoltageFirstDerivative = null!;
            branch.CapacityVoltageSecondDerivative = null!;
        }

        foreach (var graph in graphs)
        {
            /* 1. Detect variables */
            var eqSys = AddVariablesToSystemFromGraph(graph);

            /* 2. Fill equations by nodes */
            var equationCount = FillEquationsFromNodes(graph, eqSys);

            /* 2. Fill equations by circuits */
            Console.WriteLine($"nodeEquationCount = {equationCount}, circuits = {graph.Circuits.Count}");

            foreach (var circuit in graph.Circuits)
            {
                var firstBranch = circuit.Branches[0];
                var circuitNode = firstBranch.NodeLeft;
                
                var matVars = (List<ExpressionVariable>)eqSys.Variables;
                var circuitBranch = firstBranch;
                var equationNumber = equationCount++;
                
                for (var j = 0; j < eqSys.Matrix[equationNumber].Length; j++)
                {
                    eqSys.Matrix[equationNumber][j] = new ExpressionValue(0);
                }
                
                do
                {
                    var isCoDirected = circuitBranch!.NodeLeft == circuitNode;
                    var currentIndex = matVars.IndexOf(circuitBranch.Current);
                    
                    if (currentIndex != -1)
                    {
                        eqSys.Matrix[equationNumber][currentIndex] =
                            new ExpressionValue(circuitBranch.Resistance.Value);
                    }

                    //FillDcSourceVariables(equationNumber, circuitBranch, eqSys);

                    var nextNode = isCoDirected
                        ? circuitBranch.NodeRight
                        : circuitBranch.NodeLeft;

                    circuitBranch = nextNode.Branches
                        .FirstOrDefault(x => circuit.Branches.Contains(x) && x != circuitBranch);

                } while (circuitBranch != firstBranch);
            }

            equationSystems.Add(eqSys);
        }

        // var equationSystem = new EquationSystem(_i2, _i3, _i1Derivative, _ucDerivative)
        // {
        //     Matrix = new []
        //     {
        //         new Expression[] { new ExpressionValue(-1), new ExpressionValue(-1), new ExpressionValue(0), new ExpressionValue(0), -_i1 },
        //         new Expression[] { new ExpressionValue(1), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _e - (_i1 * _r) },
        //         new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_c), -_uc },
        //         new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _i1 },
        //     }
        // };
        //
        // equationSystems.Add(equationSystem);

        return equationSystems;
    }

    private EquationSystem AddVariablesToSystemFromGraph(Graph graph)
    {
        var variables = new List<ExpressionVariable>();
        
        var graphBranches = graph.Circuits.SelectMany(circuit => circuit.Branches).ToList();

        foreach (var circuit in graph.Circuits)
        {
            for (var i = 0; i < circuit.Branches.Count; i++)
            {
                var branch = circuit.Branches[i];

                if (branch.Current == null!)
                {
                    branch.Current = new ExpressionVariable
                    {
                        Label = $"i<sub-i>{graphBranches.IndexOf(branch)}</sub-i>",
                        Payload = branch
                    };
                }

                if (branch.Capacity == null! && branch.Inductance == null! && branch.Resistance != null!)
                {
                    if (!variables.Contains(branch.Current))
                    {
                        variables.Add(branch.Current);
                    }
                }

                if (branch.Capacity != null! && branch.Inductance == null!)
                {
                    var capacitorNumbers = string.Join(",", branch.Capacity.Capacitors.Select(x => x.Number));

                    if (branch.CapacityVoltage == null!)
                    {
                        branch.CapacityVoltage = new ExpressionVariable
                        {
                            Label = $"U<i>C<sub-i>{capacitorNumbers}<sub-i/></i>",
                            Payload = branch
                        };

                        branch.CapacityVoltageFirstDerivative = new ExpressionDerivative
                        {
                            Variable = branch.CapacityVoltage,
                            Payload = branch
                        };

                        variables.Add(branch.CapacityVoltageFirstDerivative);
                    }
                }

                if (branch.Capacity == null! && branch.Inductance != null!)
                {
                    if (branch.CurrentDerivative == null!)
                    {
                        branch.CurrentDerivative = new ExpressionDerivative
                        {
                            Variable = branch.Current,
                            Payload = branch
                        };

                        variables.Add(branch.CurrentDerivative);
                    }
                }

                if (branch.Capacity != null! && branch.Inductance != null!)
                {
                    // replacement

                    if (branch.CapacityVoltageFirstDerivative == null!)
                    {
                        var capacitorNumbers = string.Join(",", branch.Capacity.Capacitors.Select(x => x.Number));

                        branch.CapacityVoltage = new ExpressionVariable
                        {
                            Label = $"U<i>C<sub-i>{capacitorNumbers}<sub-i/></i>",
                            Payload = branch
                        };

                        branch.CapacityVoltageFirstDerivative = new ExpressionDerivative
                        {
                            Variable = branch.CapacityVoltage,
                            Payload = branch
                        };

                        branch.CapacityVoltageSecondDerivative = new ExpressionDerivative
                        {
                            Variable = branch.CapacityVoltageFirstDerivative,
                            Payload = branch
                        };

                        variables.Add(branch.CapacityVoltageFirstDerivative);
                        variables.Add(branch.CapacityVoltageSecondDerivative);
                    }
                }
            }
        }

        variables = variables.OrderBy(x => x.GetType() == typeof(ExpressionDerivative)).ToList();
        var eqSys = new EquationSystem(variables.ToArray());

        return eqSys;
    }

    private int FillEquationsFromNodes(Graph graph, EquationSystem equationSystem)
    {
        var nodes = new HashSet<Node>();
        var graphBranches = new List<Branch>();

        foreach (var branch in graph.Circuits.SelectMany(circuit => circuit.Branches))
        {
            graphBranches.Add(branch);
            nodes.Add(branch.NodeLeft);
            nodes.Add(branch.NodeRight);
        }
            
        var equationCount = 0;
        var mat = equationSystem.Matrix;
        var matVars = (List<ExpressionVariable>)equationSystem.Variables;
        var nodesList = nodes.ToList();
            
        /* 2.1. Currents in nodes */
            
        // MAYBE EXCLUSION LOGIC SHOULD BE ADDED
        for (var i = 0; i < nodesList.Count - 1; i ++)
        {
            var node = nodesList[i];
            var equationNumber = equationCount++;
            
            // fill equation with zeros
            for (var j = 0; j < mat.Length + 1; j++)
            {
                mat[equationNumber][j] = new ExpressionValue(0);
            }

            var branches = node.Branches.Where(x => graphBranches.Contains(x));

            foreach (var branch in branches)
            {
                // find sign for current 
                var input = branch.NodeRight == node; // sign +
                var indexOfCurrent = matVars.IndexOf(branch.Current);
                        
                if (indexOfCurrent != -1)
                {
                    mat[equationNumber][indexOfCurrent] = new ExpressionValue(input ? 1 : -1);
                }
                else
                {
                    mat[equationNumber][matVars.Count] -= (input ? 1 : -1) * branch.Current; // right side
                }
            }
        }

        return equationCount;
    }

    private void FillDcSourceVariables(int equationNumber, Branch circuitBranch, EquationSystem equationSystem)
    {
        var currentElement = circuitBranch.NodeLeft.NodeElements
            .FirstOrDefault(x => circuitBranch.Elements.Contains(x.Element))!.Element;

        var point = currentElement.Points[0];

        while (currentElement != null) // jump over branch and find all of the DC-s (Diodes in future)
        {
            if (currentElement is DCSource dcSource)
            {
                var pointIndex = ((List<Vec2>) currentElement.Points).IndexOf(point);

                var multiplier = -1;
                
                if (pointIndex == 0) // left
                {
                    if (dcSource.Direction is Direction.TOP or Direction.RIGHT)
                    {
                        multiplier = 1;
                    }
                }
                else // right
                {
                    if (dcSource.Direction is Direction.BOTTOM or Direction.LEFT)
                    {
                        multiplier = 1;
                    }
                }
                
                equationSystem.Matrix[equationNumber][equationSystem.Matrix.Length] =
                    new ExpressionValue(circuitBranch.Resistance.Value);
            }

            var hashCode = GetPointHashCode(point!);
            var node = _schemeService.Nodes[hashCode];
            
            currentElement = node.NodeElements
                .FirstOrDefault(
                    x => x.Element != currentElement && 
                         circuitBranch.Elements.Contains(x.Element))?.Element;
            
            point = currentElement?.Points.FirstOrDefault(x => GetPointHashCode(x) != hashCode)!;
        }
    }

    private int GetPointHashCode(Vec2 point)
    {
        return ((int)point.X << 16) | (int)point.Y;
    }
    
    public string PerformGaussianElimination(EquationSystem equationSystem)
    {
        var matrixLength = equationSystem.Matrix.Length;
        var mat = equationSystem.Matrix;

        for (var k = 0; k < matrixLength; k++)
        {
            // Initialize maximum value and index for pivot
            var iMax = k;
            var vMax = mat[iMax][k] is ExpressionValue expVal ? Math.Abs(expVal.Value) : 0;

            /* find greater amplitude for pivot if any */
            for (var i = k + 1; i < matrixLength; i++)
            {
                if (mat[i][k] is ExpressionValue exVal && Math.Abs(exVal.Value) > vMax)
                {
                    vMax = Math.Abs(exVal.Value);
                    iMax = i;
                }

                /* If a principal diagonal element  is zero,
                *  it denotes that matrix is singular, and
                *  will lead to a division-by-zero later. */
                if (mat[iMax][k] is ExpressionValue { Value: 0 })
                {
                    return mat[k][matrixLength] is ExpressionValue { Value: 0 }
                        ? "May have infinitely many solutions"
                        : "Inconsistent System";
                }
            }

            /* Swap the greatest value row with
               current row
            */
            if (iMax != k)
            {
                (mat[k], mat[iMax]) = (mat[iMax], mat[k]);
            }

            for (var i = k + 1; i < matrixLength; i++)
            {
                /* factor f to set current row kth element
                *  to 0, and subsequently remaining kth
                *  column to 0 */
                var f = mat[i][k] / mat[k][k];

                /* subtract fth multiple of corresponding
                   kth row element*/
                for (var j = k + 1; j <= matrixLength; j++)
                {
                    mat[i][j] -= mat[k][j] * f;
                }

                /* filling lower triangular matrix with
                *  zeros*/
                mat[i][k] = new ExpressionValue(0);
            }
        }

        return "Success";
    }
}