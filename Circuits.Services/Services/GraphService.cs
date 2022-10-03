using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Math;

namespace Circuits.Services.Services;

public class GraphService : IGraphService
{
    private readonly ISchemeService _schemeService;

    private readonly List<Branch> _branches;
    private readonly HashSet<Element> _checkedElements = new();

    public GraphService(ISchemeService schemeService)
    {
        _schemeService = schemeService;
        _branches = (List<Branch>) _schemeService.Branches;
    }

    public void BuildBranches()
    {
        _checkedElements.Clear();
        _branches.Clear();
        
        foreach (var element in _schemeService.Elements)
        {
            if (_checkedElements.Contains(element)) continue;

            var branch = new Branch();

            BuildSubbranch(branch, element, element.Points[0]); // go left
            BuildSubbranch(branch, element, element.Points[1]); // go right

            _branches.Add(branch);
        }
    }

    private void BuildSubbranch(Branch branch, Element element, Vec2 point)
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
        throw new NotImplementedException();
    }

    public void FindFundamentalCycles()
    {
        throw new NotImplementedException();
    }
}