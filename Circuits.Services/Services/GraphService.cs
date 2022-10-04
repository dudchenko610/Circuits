using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Math;

namespace Circuits.Services.Services;

public class GraphService : IGraphService
{
    private readonly ISchemeService _schemeService;

    private readonly List<Branch> _branches;
    private readonly List<Branch> _spanningTree;
    private readonly List<Branch> _leftoverBranches;
    private readonly List<Circuit> _circuits;

    private readonly HashSet<Element> _checkedElements = new();
    private readonly HashSet<Node> _spanningTreeNodes = new();

    public GraphService(ISchemeService schemeService)
    {
        _schemeService = schemeService;
        _branches = (List<Branch>) _schemeService.Branches;
        _spanningTree = (List<Branch>) _schemeService.SpanningTree;
        _leftoverBranches = (List<Branch>) _schemeService.LeftoverBranches;
        _circuits = (List<Circuit>) _schemeService.Circuits;
    }

    public void BuildBranches()
    {
        _checkedElements.Clear();
        _branches.Clear();

        foreach (var node in _schemeService.Nodes)
        {
            node.Value.Branches.Clear();
        }
        
        foreach (var element in _schemeService.Elements)
        {
            if (_checkedElements.Contains(element)) continue;

            var branch = new Branch();

            BuildSubbranch(branch, element, element.Points[0], true); // go left
            BuildSubbranch(branch, element, element.Points[1], false); // go right

            _branches.Add(branch);
        }
    }

    private void BuildSubbranch(Branch branch, Element element, Vec2 point, bool left)
    {
        while (element != null!)
        {
            if (!branch.Elements.Contains(element))
            {
                branch.Elements.Add(element);
            }

            _checkedElements.Add(element);
            
            var hashCode = GetPointHashCode(point!);
            var node = _schemeService.Nodes[hashCode];

            if (node.NodeElements.Count != 2)
            {
                element = null!;
                // save point as branch point

                if (left)
                {
                    branch.NodeLeft = node;
                }
                else
                {
                    branch.NodeRight = node;
                }

                node.Branches.Add(branch);
            }
            else
            {
                element = node.NodeElements.FirstOrDefault(x => x.Element != element)!.Element;
                point = element.Points.FirstOrDefault(x => GetPointHashCode(x) != hashCode)!;
            }
        }
    }

    private int GetPointHashCode(Vec2 point)
    {
        return ((int)point.X << 16) | (int)point.Y;
    }

    public void BuildSpanningTree()
    {
        if (_branches.Count == 0) return;
        
        _spanningTreeNodes.Clear();
        _spanningTree.Clear();
        _leftoverBranches.Clear();

        _spanningTreeNodes.Add(_branches[0].NodeLeft);
        
        foreach (var branch in _branches)
        {
            var leftNode = _spanningTreeNodes.Contains(branch.NodeLeft);
            var rightNode = _spanningTreeNodes.Contains(branch.NodeRight);
            
            if (leftNode && !rightNode)
            {
                _spanningTree.Add(branch);
                _spanningTreeNodes.Add(branch.NodeRight);
            }

            if (!leftNode && rightNode)
            {
                _spanningTree.Add(branch);
                _spanningTreeNodes.Add(branch.NodeLeft);
            }

            if (
                _spanningTreeNodes.Contains(branch.NodeLeft) && 
                _spanningTreeNodes.Contains(branch.NodeRight) &&
                !_spanningTree.Contains(branch))
            {
                _leftoverBranches.Add(branch);
            }
        }
    }

    public void FindFundamentalCycles()
    {
        _circuits.Clear();
        
        var traversed = new List<Branch>();
        
        foreach (var branch in _leftoverBranches)
        {
            var circuit = new Circuit();
            
            circuit.Branches.Add(branch);
            circuit.Branches.AddRange(_spanningTree);
            traversed.Clear();
            
            TraverseGraph(circuit, traversed, branch, true);
            
            _circuits.Add(circuit);
        }
    }

    private void TraverseGraph(Circuit circuit, List<Branch> traversed, Branch branch, bool checkLeft)
    {
        // 1. Check traversed and add if is not
        if (traversed.Contains(branch)) return;
        traversed.Add(branch);
        
        // 2. Traverse children
        var node = checkLeft ? branch.NodeLeft : branch.NodeRight;

        foreach (var childBranch in node.Branches)
        {
            if (childBranch == branch || !circuit.Branches.Contains(branch)) continue;
            
            TraverseGraph(circuit, traversed, childBranch, childBranch.NodeRight == node);
        }

        // 3. Remove if leaf
        //var oppositeNode = checkLeft ? branch.NodeRight : branch.NodeLeft;
        var branchesLeftCount = node.Branches
            .Count(x => x != branch && traversed.Contains(x) && circuit.Branches.Contains(x));

        if (branchesLeftCount == 0)
        {
            circuit.Branches.Remove(branch);
        }
    }
}