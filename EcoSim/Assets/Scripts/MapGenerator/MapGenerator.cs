using System.Collections.Generic;
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
        FindRivers(graph, 12f);
        CreateLakes(graph);

        AddMountains(graph, 4.5f, 5f, 8f, 6.5f, 11.5f);
        AddTallGrass(graph, 2f, 2.5f);

        // Average the center points
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
            .FirstOrDefault(x => x.Value.IsEdge()
                            && x.Value.nodeType == MapGraph.NodeType.FreshWater)
            .Value;

        FloodFill(startNode, MapGraph.NodeType.FreshWater, MapGraph.NodeType.SaltWater);
    }

    private static void FloodFill(MapGraph.Node node, MapGraph.NodeType targetType, MapGraph.NodeType replacementType)
    {
        if (targetType == replacementType
            || node.nodeType != targetType)
        {
            return;
        }

        node.nodeType = replacementType;
        foreach (var neighbor in node.GetNeighborNodes())
        {
            FloodFill(neighbor, targetType, replacementType);
        }
    }

    private static void SetBeaches(MapGraph graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.nodeType != MapGraph.NodeType.Grass)
            {
                continue;
            }

            foreach (var neighbor in node.GetNeighborNodes())
            {
                if (neighbor.nodeType == MapGraph.NodeType.SaltWater)
                {
                    node.nodeType = MapGraph.NodeType.Beach;
                    break;
                }
            }
        }
    }

    private static void FindRivers(MapGraph graph, float minElevation)
    {
        var riverCount = 0;
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.elevation > minElevation)
            {
                var waterSource = node.GetLowestCorner();
                var lowestEdge  = waterSource.GetDownSlopeEdge();

                if (lowestEdge == null)
                {
                    continue;
                }

                CreateRiver(graph, lowestEdge);
                riverCount++;
            }
        }
    }

    private static void CreateRiver(MapGraph graph, MapGraph.Edge startEdge)
    {
        bool heightUpdated = false;

        // Once a river has been generated, it tries again to see if a quicker
        // route has been created. This sets how many times we should go over
        // the same river.
        var maxIterations  = 1;
        var iterationCount = 0;

        // Make sure that the river generation code doesn't get stuck in a loop.
        var maxChecks  = 100;
        var checkCount = 0;

        var previousRiverEdges = new List<MapGraph.Edge>();
        do
        {
            heightUpdated = false;

            var riverEdges   = new List<MapGraph.Edge>();
            var previousEdge = startEdge;
            var nextEdge     = startEdge;

            while (nextEdge != null)
            {
                if (checkCount >= maxChecks)
                {
                    // Unable to find route for river, maximum number of checks reached.
                    return;
                }

                checkCount++;
                var currentEdge = nextEdge;

                // We've already seen this edge and it's flowing back up itself.
                if (riverEdges.Contains(currentEdge)
                    || riverEdges.Contains(currentEdge.opposite))
                {
                    break;
                }

                riverEdges.Add(currentEdge);
                currentEdge.water++;

                // Check that we haven't reached the sea
                var edgeNodes = currentEdge.destination.GetEdges().Select(x => x.node);
                if (edgeNodes.Any(x => x.nodeType == MapGraph.NodeType.SaltWater))
                {
                    break;
                }

                nextEdge = currentEdge.GetDownSlopeEdge(riverEdges);

                if (nextEdge == null
                    && previousEdge != null)
                {
                    // We need to start carving a path for the river.
                    nextEdge = GetNewCandidateEdge(graph.center, currentEdge, riverEdges, previousRiverEdges);

                    // If we can't get a candidate edge, then backtrack and try again
                    var previousEdgeIndex = riverEdges.Count - 1;
                    while (nextEdge == null
                           || previousEdgeIndex == 0)
                    {
                        previousEdge = riverEdges[previousEdgeIndex];
                        previousEdge.water--;

                        nextEdge = GetNewCandidateEdge(graph.center, previousEdge, riverEdges, previousRiverEdges);

                        riverEdges.Remove(previousEdge);
                        previousEdgeIndex--;
                    }

                    if (nextEdge != null)
                    {
                        if (nextEdge.previous.destination.position.y != nextEdge.destination.position.y)
                        {
                            // Level the edge
                            nextEdge.destination.position = new Vector3(nextEdge.destination.position.x, nextEdge.previous.destination.position.y, nextEdge.destination.position.z);
                            heightUpdated = true;
                        }
                    }
                    else
                    {
                        // We've tried tunneling, backtracking, and we're still lost.
                        Debug.LogError("Unable to find route for river");
                    }
                }

                previousEdge = currentEdge;
            }

            if (maxIterations <= iterationCount)
            {
                break;
            }
            iterationCount++;

            // If the height was updated, we need to recheck the river again.
            if (heightUpdated)
            {
                foreach (var edge in riverEdges)
                {
                    if (edge.water > 0)
                    {
                        edge.water--;
                    }
                }

                previousRiverEdges = riverEdges;
            }
        } while (heightUpdated);
    }

    private static MapGraph.Edge GetNewCandidateEdge(
        Vector3 center,
        MapGraph.Edge source,
        List<MapGraph.Edge> seenEdges,
        List<MapGraph.Edge> previousEdges
    )
    {
        var corner = source.destination;

        var edges = corner.GetEdges()
            .Where(x => !seenEdges.Contains(x)
                   && x.opposite != null
                   && !seenEdges.Contains(x.opposite))
            .ToList();

        // Make sure the river prefers to follow existing rivers
        var existingRiverEdge = edges.FirstOrDefault(x => x.water > 0);
        if (existingRiverEdge != null)
        {
            return existingRiverEdge;
        }

        // Make the river prefer to follow previous iterations
        existingRiverEdge = edges.FirstOrDefault(x => previousEdges.Contains(x));
        if (existingRiverEdge != null)
        {
            return existingRiverEdge;
        }

        var awayFromCenterEdges = edges
            .Where(x => Vector3.Dot(x.destination.position - x.previous.destination.position, x.destination.position - center) >= 0);

        if (awayFromCenterEdges.Any())
        {
            edges = awayFromCenterEdges.ToList();
        }

        return edges.OrderBy(x => x.destination.position.y).FirstOrDefault();
    }

    private static void CreateLakes(MapGraph graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            var edges = node.GetEdges();
            if (!edges.Any(x => x.water == 0)
                || edges.Where(x => x.water > 0).Count() > 3)
            {
                var lowestCorner = node.GetLowestCorner();
                node.nodeType    = MapGraph.NodeType.FreshWater;

                // Set all of the heights equal to where the water came in.
                node.SetNodeHeightToCornerHeight(lowestCorner);
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
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.nodeType != MapGraph.NodeType.Grass)
            {
                continue;
            }

            if (node.elevation > minElevation
                || node.GetHeightDifference() > minHeightDifference)
            {
                node.nodeType = MapGraph.NodeType.TallGrass;
                continue;
            }

            foreach (var neighbor in node.GetNeighborNodes())
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
