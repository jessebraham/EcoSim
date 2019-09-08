using System.Linq;
using UnityEngine;

public static class MapGenerator
{
    public static void GenerateMap(MapGraph graph)
    {
        // Initially, set all nodes to Grass.
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            node.nodeType = MapGraph.NodeType.Grass;
        }

        SetLowNodesToWater(graph, 0.4f);
        FillOcean(graph);
        SetBeaches(graph);

        AddMountains(graph, 5f, 5f, 8f, 6.5f, 11.5f);
        AddTallGrass(graph, 2f, 2.5f);

        // Average the center points based on the average height of the corner
        // vertices.
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            node.centerPoint = new Vector3(
                node.centerPoint.x,
                node.GetCorners().Average(x => x.position.y),
                node.centerPoint.z
            );
        }
    }


    private static void SetLowNodesToWater(MapGraph graph, float cutoff)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            // Skip any nodes whose elevation exceed the `cutoff` value.
            if (node.centerPoint.y > cutoff)
            {
                continue;
            }

            // If any edge elevations exceed the cutoff, we will not set the
            // node to FreshWater.
            var allZero = true;
            foreach (var edge in node.GetEdges())
            {
                if (edge.destination.position.y > cutoff)
                {
                    allZero = false;
                    break;
                }
            }

            if (allZero)
            {
                node.nodeType = MapGraph.NodeType.FreshWater;
            }
        }
    }

    private static void FillOcean(MapGraph graph)
    {
        var startNode = graph.nodesByCenterPosition
            .FirstOrDefault(node => node.Value.IsEdge()
                            && node.Value.nodeType == MapGraph.NodeType.FreshWater)
            .Value;

        FloodFill(startNode, MapGraph.NodeType.FreshWater, MapGraph.NodeType.SaltWater);
    }

    private static void FloodFill(
        MapGraph.Node node,
        MapGraph.NodeType targetType,
        MapGraph.NodeType replacementType
    )
    {
        if (targetType == replacementType
            || node.nodeType != targetType)
        {
            return;
        }

        node.nodeType = replacementType;
        foreach (var neighbor in node.GetNeighbourNodes())
        {
            FloodFill(neighbor, targetType, replacementType);
        }
    }

    private static void SetBeaches(MapGraph graph)
    {
        foreach (var node in graph.FilterNodes(MapGraph.NodeType.Grass))
        {
            foreach (var neighbour in node.GetNeighbourNodes())
            {
                if (neighbour.nodeType == MapGraph.NodeType.SaltWater)
                {
                    node.nodeType = MapGraph.NodeType.Beach;
                    break;
                }
            }
        }
    }

    private static void AddMountains(
        MapGraph graph,
        float minRockyElevation,
        float minRockyHeightDifference,
        float minElevation,
        float minHeightDifference,
        float minSnowElevation
    )
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.elevation > minElevation
                || node.GetHeightDifference() > minHeightDifference)
            {
                node.nodeType = MapGraph.NodeType.Mountain;
            }
            else if (node.elevation > minRockyElevation
                     || node.GetHeightDifference() > minRockyHeightDifference)
            {
                node.nodeType = MapGraph.NodeType.Rocky;
            }

            if (node.elevation > minSnowElevation)
            {
                node.nodeType = MapGraph.NodeType.Snow;
            }
        }
    }

    private static void AddTallGrass(MapGraph graph, float minElevation, float minHeightDifference)
    {
        foreach (var node in graph.FilterNodes(MapGraph.NodeType.Grass))
        {
            if (node.elevation > minElevation
                || node.GetHeightDifference() > minHeightDifference)
            {
                node.nodeType = MapGraph.NodeType.TallGrass;
                continue;
            }

            foreach (var neighbor in node.GetNeighbourNodes())
            {
                if (neighbor.nodeType == MapGraph.NodeType.FreshWater)
                {
                    node.nodeType = MapGraph.NodeType.TallGrass;
                    break;
                }
            }
        }        
    }
}
