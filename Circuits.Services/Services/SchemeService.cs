using System.Drawing;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;
using Circuits.ViewModels.Math;

namespace Circuits.Services.Services;

public class SchemeService : ISchemeService
{
    class NodeData
    {
        public Element Element { get; set; }
        public int PointIndex { get; set; }
    }

    public IReadOnlyList<Element> Elements { get; }

    public event Action? OnUpdate;
    private readonly List<Element> _elements = new();

    private Dictionary<int, List<NodeData>> _nodesHashMap = new();

    public SchemeService()
    {
        Elements = _elements;
    }

    public void Add(Element element)
    {
        // 1. Find touching sides

        foreach (var point in element.Points)
        {
            var isHorizontal = element.IsHorizontal(point);

            SeparateWire(point, isHorizontal);
        }

        if (element is Wire wire)
        {
            var isHorizontal = wire.IsHorizontal();
            
            // 3. Join parallel
            
            var hashCodeP1 = ((int) wire.P1.X << 16) | (int) wire.P1.Y;
            var hashCodeP2 = ((int) wire.P2.X << 16) | (int) wire.P2.Y;
            var nodeDataListP1 = GetNodeData(hashCodeP1);
            var nodeDataListP2 = GetNodeData(hashCodeP2);

            if (nodeDataListP1.Count == 1 && nodeDataListP1[0].Element is Wire leftWire && nodeDataListP1[0].Element.IsHorizontal() == isHorizontal)
            {
                wire.P1.Set(leftWire.P1);
                RemoveWithoutCheck(leftWire);
            }
            
            if (nodeDataListP2.Count == 1 && nodeDataListP2[0].Element is Wire rightWire && nodeDataListP2[0].Element.IsHorizontal() == isHorizontal)
            {
                wire.P2.Set(rightWire.P2);
                RemoveWithoutCheck(rightWire);
            }

            // 4. Separate by others wires
            
            var dividePoints = new List<Vec2>();

            if (isHorizontal)
            {
                for (var i = (int) wire.P1.X + 1; i < (int) wire.P2.X; i ++)
                {
                    var hashCode = (i << 16) | (int) wire.P1.Y;
                    var nodeDataList = GetNodeData(hashCode);

                    if (nodeDataList.Count == 0) continue;
                    // should be perpendicular, in theory

                    dividePoints.Add(new Vec2(i, wire.P1.Y));
                }
            }
            else
            {
                for (var i = (int) wire.P1.Y + 1; i < (int) wire.P2.Y; i ++)
                {
                    var hashCode = ((int) wire.P1.X << 16) | i;
                    var nodeDataList = GetNodeData(hashCode);

                    if (nodeDataList.Count == 0) continue;
                    // should be perpendicular, in theory

                    dividePoints.Add(new Vec2(wire.P1.X, i));
                }
            }
            
            dividePoints.Insert(0, wire.P1);
            dividePoints.Add(wire.P2);

            for (var i = 0; i < dividePoints.Count - 1; i++)
            {
                AddWithoutCheck(new Wire
                {
                    P1 = dividePoints[i + 0],
                    P2 = dividePoints[i + 1]
                });
            }
        }
        else
        {
            AddWithoutCheck(element);
        }
    }

    private void SeparateWire(Vec2 point, bool isHorizontal)
    {
        foreach (var el in _elements)
        {
            if (el is Wire existingWire)
            {
                var exElIsHorizontal = existingWire.IsHorizontal();

                if (isHorizontal != exElIsHorizontal)
                {
                    if (isHorizontal && (int)point.X == (int)existingWire.P1.X && point.Y > existingWire.P1.Y &&
                        point.Y < existingWire.P2.Y) // existing is vertical, touches side by vertex
                    {
                        RemoveWithoutCheck(existingWire);

                        var wire1 = new Wire
                        {
                            P1 = existingWire.P1,
                            P2 = new Vec2
                            {
                                X = existingWire.P1.X,
                                Y = point.Y
                            }
                        };

                        var wire2 = new Wire
                        {
                            P1 = new Vec2
                            {
                                X = existingWire.P1.X,
                                Y = point.Y
                            },
                            P2 = existingWire.P2,
                        };

                        AddWithoutCheck(wire1);
                        AddWithoutCheck(wire2);

                        break;
                    }
                    else if ((int)point.Y == (int)existingWire.P1.Y && point.X > existingWire.P1.X && point.X < existingWire.P2.X)
                    {
                        RemoveWithoutCheck(existingWire);

                        var wire1 = new Wire
                        {
                            P1 = existingWire.P1,
                            P2 = new Vec2
                            {
                                X = point.X,
                                Y = existingWire.P1.Y
                            }
                        };

                        var wire2 = new Wire
                        {
                            P1 = new Vec2
                            {
                                X = point.X,
                                Y = existingWire.P1.Y
                            },
                            P2 = existingWire.P2,
                        };

                        AddWithoutCheck(wire1);
                        AddWithoutCheck(wire2);

                        break;
                    }
                }
            }
        }
    }

    public void Remove(Element element)
    {
        RemoveWithoutCheck(element);
        
        foreach (var point in element.Points)
        {
            var hashCode = ((int)point.X << 16) | (int)point.Y;
            var nodeDataList = GetNodeData(hashCode);

            if (nodeDataList.Count == 2 && 
                nodeDataList[0].Element is Wire w1 && nodeDataList[1].Element is Wire w2 && 
                w1.IsHorizontal() == w2.IsHorizontal())
            {
                if (nodeDataList[0].PointIndex == 0)
                {
                    RemoveWithoutCheck(w1);
                    RemoveWithoutCheck(w2);
                    w1.P1.Set(w2.P1);
                    AddWithoutCheck(w1);
                }
                else
                {
                    RemoveWithoutCheck(w1);
                    RemoveWithoutCheck(w2);
                    w2.P1.Set(w1.P1);
                    AddWithoutCheck(w2);
                }
            }
        }
    }

    public bool Overlap(Element e1, Element e2)
    {
        return true;
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }

    public bool Intersects(Element element)
    {
        foreach (var el in _elements)
        {
            if (el != element && Intersects(el, element))
                return true;
        }

        return false;
    }

    private void RemoveWithoutCheck(Element element)
    {
        _elements.Remove(element);

        foreach (var point in element.Points)
        {
            var hashCode = ((int)point.X << 16) | (int)point.Y;
            var nodeDataList = GetNodeData(hashCode);

            var nodeData = nodeDataList.FirstOrDefault(x => x.Element == element);

            if (nodeData is not null)
            {
                nodeDataList.Remove(nodeData);
            }
        }
    }

    private void AddWithoutCheck(Element element)
    {
        _elements.Add(element);

        for (var i = 0; i < element.Points.Count; i++)
        {
            var point = element.Points[i];

            var hashCode = ((int)point.X << 16) | (int)point.Y;
            var nodeDataList = GetNodeData(hashCode);

            nodeDataList.Add(new NodeData
            {
                PointIndex = i,
                Element = element
            });
        }
    }

    private List<NodeData> GetNodeData(int hashCode)
    {
        if (_nodesHashMap.TryGetValue(hashCode, out var nodeDataList)) return nodeDataList;

        nodeDataList = new List<NodeData>();
        _nodesHashMap.Add(hashCode, nodeDataList);

        return nodeDataList;
    }

    private bool Intersects(Element e1, Element e2)
    {
        if (e1 is Wire w1 && e2 is Wire w2)
        {
            var w1IsHorizontal = (int)w1.P1.Y == (int)w1.P2.Y;
            var w2IsHorizontal = (int)w2.P1.Y == (int)w2.P2.Y;

            var areHorizontalInOneLine = w1IsHorizontal && w2IsHorizontal && (int)w1.P1.Y == (int)w2.P1.Y;
            var areVerticalInOneLine = !w1IsHorizontal && !w2IsHorizontal && (int)w1.P1.X == (int)w2.P1.X;

            return (areHorizontalInOneLine && !((w1.P2.X <= w2.P1.X) || (w2.P2.X <= w1.P1.X))) ||
                   (areVerticalInOneLine && !((w1.P2.Y <= w2.P1.Y) || (w2.P2.Y <= w1.P1.Y)));
        }

        return false;
    }
}