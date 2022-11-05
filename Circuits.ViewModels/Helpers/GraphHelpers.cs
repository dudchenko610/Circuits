using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Entities.Structures;
using Circuits.ViewModels.Math;

namespace Circuits.ViewModels.Helpers;

public static class GraphHelpers
{
    public static bool IsCoDirected(Circuit circuit, Branch branch)
    {
        var firstBranch = circuit.Branches[0];
        var circuitBranch = firstBranch;
        var circuitNode = firstBranch.NodeLeft;
        
        do
        {
            var isCoDirected = circuitBranch!.NodeLeft == circuitNode; // current is coDirected

            if (circuitBranch == branch) return isCoDirected;

            var nextNode = isCoDirected
                ? circuitBranch.NodeRight
                : circuitBranch.NodeLeft;

            circuitBranch = nextNode.Branches
                .FirstOrDefault(x => circuit.Branches.Contains(x) && x != circuitBranch);
            circuitNode = nextNode;
            
        } while (circuitBranch != firstBranch);

        throw new Exception("Branch doesn't belong to circuit");
    }

    public static bool IsCoDirected(IReadOnlyDictionary<int, Node> nodes, Branch branch, Element element)
    {
        var currentNodeElement = branch.NodeLeft.NodeElements
            .FirstOrDefault(x => branch.Elements.Contains(x.Element));

        var currentElement = currentNodeElement!.Element;
        var pointIndex = currentNodeElement!.PointIndex; 
        // var point = currentElement.Points[pointIndex]; // we approach from left side of branch to DC
        
        while (currentElement != null!) // jump over branch and find all of the DC-s (Diodes in future)
        {
            var isCoDirected = pointIndex == 0;

            if (currentElement == element)
            {
                if (element.Direction is Direction.TOP or Direction.LEFT) isCoDirected = !isCoDirected;
                return isCoDirected;
            }

            var nextPoint = currentElement.Points[(pointIndex + 1) % 2];
            var node = nodes[GetPointHashCode(nextPoint)];
            
            currentNodeElement = node.NodeElements
                .FirstOrDefault(
                    x => x.Element != currentElement && 
                         branch.Elements.Contains(x.Element));
            
            if (currentNodeElement is null) break;
            
            currentElement = currentNodeElement.Element;
            pointIndex = currentNodeElement.PointIndex;

            // point = currentElement.Points[pointIndex];
        }

        throw new Exception("Element doesn't belong to branch");
    }
    
    public static void IterateCircuit(this Circuit circuit, Action<Branch, bool> action)
    {
        var firstBranch = circuit.Branches[0];
        var circuitBranch = firstBranch;
        var circuitNode = firstBranch.NodeLeft;
        
        do
        {
            var isCoDirected = circuitBranch!.NodeLeft == circuitNode; // current is coDirected
            action.Invoke(circuitBranch, isCoDirected);

            var nextNode = isCoDirected
                ? circuitBranch.NodeRight
                : circuitBranch.NodeLeft;

            circuitBranch = nextNode.Branches
                .FirstOrDefault(x => circuit.Branches.Contains(x) && x != circuitBranch);
            
            if (circuitBranch is null) return;
            
            circuitNode = nextNode;
            
        } while (circuitBranch != firstBranch);
    }
    
    public static void IterateBranch(this Branch branch, IReadOnlyDictionary<int, Node> nodes, Action<Element, bool> action)
    {
        var currentNodeElement = branch.NodeLeft.NodeElements
            .FirstOrDefault(x => branch.Elements.Contains(x.Element));

        var currentElement = currentNodeElement!.Element;
        var pointIndex = currentNodeElement!.PointIndex; 
        
        while (currentElement != null!) // jump over branch and find all of the DC-s (Diodes in future)
        {
            var isCoDirected = pointIndex == 0;
            if (currentElement.Direction is Direction.TOP or Direction.LEFT) isCoDirected = !isCoDirected;
            
            action.Invoke(currentElement, isCoDirected);

            var nextPoint = currentElement.Points[(pointIndex + 1) % 2];
            var node = nodes[GetPointHashCode(nextPoint)];
            
            currentNodeElement = node.NodeElements
                .FirstOrDefault(
                    x => x.Element != currentElement && 
                         branch.Elements.Contains(x.Element));
            
            if (currentNodeElement is null) break;
            
            currentElement = currentNodeElement.Element;
            pointIndex = currentNodeElement.PointIndex;
        }
    }
    
    public static int GetPointHashCode(Vec2 point)
    {
        return ((int)point.X << 16) | (int)point.Y;
    }
}