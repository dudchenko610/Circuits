using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Entities.Structures.Properties;
using Circuits.ViewModels.Helpers;
using Circuits.ViewModels.Math;

namespace Circuits.Services.Services;

public class GraphService : IGraphService
{
    private readonly ISchemeService _schemeService;

    private readonly List<Branch> _branches;
    private readonly List<Graph> _graphs;

    private readonly HashSet<Element> _checkedElements = new();

    public GraphService(ISchemeService schemeService)
    {
        _schemeService = schemeService;
        _branches = (List<Branch>)_schemeService.Branches;
        _graphs = (List<Graph>)_schemeService.Graphs;
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

            var hashCode = GraphHelpers.GetPointHashCode(point!);
            var node = _schemeService.Nodes[hashCode];
            var nextElement = node.NodeElements.FirstOrDefault(x => x.Element != element)?.Element;

            if (node.NodeElements.Count != 2 ||
                (nextElement != null! && branch.Elements.Contains(nextElement))) // branching or loop
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

                // it should happen always on right node
                if (branch.NodeLeft ==
                    branch.NodeRight) // leftover branch creates cycle, solution is to divide it into two branches
                {
                    var el1 = branch.Elements.LastOrDefault(); // guaranteed to be contained in right node 
                    branch.Elements.Remove(el1!);

                    // step back or smth like that
                    var anotherPoint =
                        el1!.Points.FirstOrDefault(x =>
                            GraphHelpers.GetPointHashCode(x) !=
                            hashCode)!; // PROBLEMATIC FOR TRANSISTORS ( or maybe not :) )
                    var anotherHashCode = GraphHelpers.GetPointHashCode(anotherPoint!);
                    var anotherNode = _schemeService.Nodes[anotherHashCode];

                    anotherNode.Branches.Add(branch);
                    branch.NodeRight = anotherNode;

                    var branch1 = new Branch();
                    branch1.Elements.Add(el1);
                    branch1.NodeLeft = anotherNode;
                    branch1.NodeRight = branch.NodeLeft;

                    anotherNode.Branches.Add(branch1);
                    branch.NodeLeft.Branches.Add(branch1);

                    _branches.Add(branch1);
                }
                else
                {
                    node.Branches.Add(branch);
                }
            }
            else
            {
                element = nextElement!;
                point = element.Points.FirstOrDefault(x => GraphHelpers.GetPointHashCode(x) != hashCode)!;
            }
        }
    }

    public void BuildSpanningTrees() // problem here
    {
        if (_branches.Count == 0) return;

        var spanningTreeNodes = new HashSet<Node>();
        var branches = _branches.ToList();
        var usedBranches = new List<Branch>();

        _graphs.Clear();

        while (branches.Count != 0)
        {
            spanningTreeNodes.Clear();
            spanningTreeNodes.Add(branches[0].NodeLeft);
            var graph = new Graph();

            TraverseSpanningTree(branches[0], graph, spanningTreeNodes, usedBranches);

            branches.RemoveAll(x => usedBranches.Contains(x));
            usedBranches.Clear();

            _graphs.Add(graph);
        }
    }

    private void TraverseSpanningTree(Branch branch, Graph graph, HashSet<Node> spanningTreeNodes,
        List<Branch> usedBranches)
    {
        var leftNode = spanningTreeNodes.Contains(branch.NodeLeft);
        var rightNode = spanningTreeNodes.Contains(branch.NodeRight);

        if (leftNode && !rightNode)
        {
            graph.SpanningTree.Add(branch);
            spanningTreeNodes.Add(branch.NodeRight);
            usedBranches.Add(branch);
        }

        if (!leftNode && rightNode)
        {
            graph.SpanningTree.Add(branch);
            spanningTreeNodes.Add(branch.NodeLeft);
            usedBranches.Add(branch);
        }

        if (
            spanningTreeNodes.Contains(branch.NodeLeft) &&
            spanningTreeNodes.Contains(branch.NodeRight) &&
            !graph.SpanningTree.Contains(branch))
        {
            graph.LeftoverBranches.Add(branch);
            usedBranches.Add(branch);
        }

        var leftBranches = branch.NodeLeft.Branches
            .Where(x => x != branch && !usedBranches.Contains(x));

        foreach (var nextBranch in leftBranches)
        {
            TraverseSpanningTree(nextBranch, graph, spanningTreeNodes, usedBranches);
        }

        var rightBranches = branch.NodeRight.Branches
            .Where(x => x != branch && !usedBranches.Contains(x));

        foreach (var nextBranch in rightBranches)
        {
            TraverseSpanningTree(nextBranch, graph, spanningTreeNodes, usedBranches);
        }
    }

    public void FindFundamentalCycles()
    {
        foreach (var graph in _graphs)
        {
            graph.Circuits.Clear();

            var traversed = new List<Branch>();

            foreach (var branch in graph.LeftoverBranches)
            {
                var circuit = new Circuit();

                circuit.Branches.Add(branch);
                circuit.Branches.AddRange(graph.SpanningTree);
                traversed.Clear();

                TraverseGraph(circuit, traversed, branch, true);

                graph.Circuits.Add(circuit);
            }

            JoinOneCircuitGraphsBranches(graph);
        }
    }

    private void JoinOneCircuitGraphsBranches(Graph graph)
    {
        //var nodes = (Dictionary<int, Node>)_schemeService.Nodes;
        var branches = (List<Branch>)_schemeService.Branches;

        if (graph.Circuits.Count != 1) return;
        
        var circuit = graph.Circuits[0];

        while (circuit.Branches.Count != 1)
        {
            var c = 0;
            Branch currentBranch = null!;
            Branch nextBranch = null!;
            var _currentIsCodirected = false;
            var _nextIsCodirected = false;

            circuit.IterateCircuit((b, coDirected) =>
            {
                switch (c)
                {
                    case 0:
                        currentBranch = b;
                        _currentIsCodirected = coDirected;
                        break;
                    case 1:
                        nextBranch = b;
                        _nextIsCodirected = coDirected;
                        break;
                }

                c++;
            });
            
            // join two branches, next will be added to current and the next will be removed

            Node nodeToRemove = null!;
            Node newNode = null!;
            
            if (_currentIsCodirected)
            {
                if (_nextIsCodirected)
                {
                    nodeToRemove = currentBranch.NodeRight; // should be the same as nextBranch.NodeLeft
                    newNode = nextBranch.NodeRight;
                    currentBranch.NodeRight = newNode;
                }
                else
                {
                    nodeToRemove = currentBranch.NodeRight; // should be the same as nextBranch.NodeRight
                    newNode = nextBranch.NodeLeft;
                    currentBranch.NodeRight = newNode;
                }
            }
            else
            {
                if (_nextIsCodirected)
                {
                    nodeToRemove = currentBranch.NodeLeft; // should be the same as nextBranch.NodeLeft
                    newNode = nextBranch.NodeRight;
                    currentBranch.NodeLeft = newNode;
                }
                else
                {
                    nodeToRemove = currentBranch.NodeLeft; // should be the same as nextBranch.NodeRight
                    newNode = nextBranch.NodeLeft;
                    currentBranch.NodeLeft = newNode;  
                }
            }
            
            // add all elements from next branch to current
            currentBranch.Elements.AddRange(nextBranch.Elements); // check for copies ???
            
            // remove old branch and add new to newNode
            newNode.Branches.Remove(nextBranch);
            
            nodeToRemove.Branches.Remove(nextBranch);
            nodeToRemove.Branches.Remove(currentBranch);
            
            if (!newNode.Branches.Contains(currentBranch)) newNode.Branches.Add(currentBranch);
                        
            // remove from scheme redundant node and branch
          //  var item = nodes.First(kvp => kvp.Value == nodeToRemove);
          //  nodes.Remove(item.Key);
            branches.Remove(nextBranch);
            
            // remove from circuit
            circuit.Branches.Remove(nextBranch);
        }
    }

    public void CollectProperties()
    {
        foreach (var branch in _schemeService.Branches)
        {
            var resistors = branch.Elements.OfType<Resistor>().ToList();
            var capacitors = branch.Elements.OfType<Capacitor>().ToList();
            var inductors = branch.Elements.OfType<Inductor>().ToList();

            if (resistors != null!)
            {
                var resistance = resistors!.Sum(resistor => resistor.Resistance);

                branch.Resistance =
                    resistors!.Count > 0 ? new Resistance { Resistors = resistors, Value = resistance } : null!;
            }

            if (capacitors != null!)
            {
                var capacity = capacitors!.Sum(capacitor => 1.0 / capacitor.Capacity);

                branch.Capacity =
                    capacitors!.Count > 0 ? new Capacity { Capacitors = capacitors, Value = capacity } : null!;
            }

            if (inductors != null!)
            {
                var inductance = inductors!.Sum(inductor => inductor.Inductance);

                branch.Inductance =
                    inductors!.Count > 0 ? new Inductance { Inductors = inductors, Value = inductance } : null!;
            }
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