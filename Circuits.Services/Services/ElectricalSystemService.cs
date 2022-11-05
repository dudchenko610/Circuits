using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Helpers;

namespace Circuits.Services.Services;

public class ElectricalSystemService : IElectricalSystemService
{
    private readonly ISchemeService _schemeService;

    public ElectricalSystemService(ISchemeService schemeService)
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
            
            branch.DCVariables.Clear();
            FillDcSourceVariables(branch);
        }

        foreach (var graph in graphs)
        {
            /* 1. Detect variables */
            var eqSys = AddVariablesToSystemFromGraph(graph);

            /* 2. Fill equations by nodes */
            var equationCount = FillEquationsFromNodes(graph, eqSys);

            /* 2. Fill equations by circuits */
            Console.WriteLine($"nodeEquationCount = {equationCount}, circuits = {graph.Circuits.Count}");

            // equationCount--;
            
            foreach (var circuit in graph.Circuits)
            {
                var matVars = (List<ExpressionVariable>)eqSys.Variables;
                var equationNumber = equationCount++;
                
                for (var j = 0; j < eqSys.Matrix[equationNumber].Length; j++)
                {
                    eqSys.Matrix[equationNumber][j] = new ExpressionValue(0);
                }
                
                circuit.IterateCircuit((branch, isCoDirected) =>
                {
                    var currentIndex = matVars.IndexOf(branch.Current);
                        
                    foreach (var dcVar in branch.DCVariables)
                    {
                        eqSys.Matrix[equationNumber][eqSys.Matrix.Length] -= (isCoDirected ? 1 : -1) * dcVar;
                    }
                    
                    if (currentIndex != -1)
                    {
                        eqSys.Matrix[equationNumber][currentIndex] = new ExpressionValue(branch.Resistance.Value);
                    }
                    
                    // place other variables
                });
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
                        
                if (indexOfCurrent != -1) mat[equationNumber][indexOfCurrent] = new ExpressionValue(input ? 1 : -1);
                else mat[equationNumber][matVars.Count] -= (input ? 1 : -1) * branch.Current; // right side
            }
        }

        return equationCount;
    }

    private void FillDcSourceVariables(Branch branch)
    {
        branch.IterateBranch(_schemeService.Nodes, (element, isCoDirected) =>
        {
            if (element is not DCSource dcSource) return;
            
            branch.DCVariables.Add((isCoDirected ? 1 : -1) * new ExpressionVariable
            {
                Label = $"ε<sub-i>{dcSource.Number}</sub-i>",
                Payload = dcSource
            });
        });
    }
}