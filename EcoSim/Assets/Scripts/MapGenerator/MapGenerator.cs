using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapGenerator
{
    public static void GenerateMap(MapGraph graph)
    {
        SetNodesToGrass(graph);

        SetLowNodesToWater(graph, 0.4f);
        FillOcean(graph);

        SetBeaches(graph);
        FindRivers(graph, 12f);
        CreateLakes(graph);

        AddMountains(graph, 4.5f, 5f, 8f, 6.5f, 13f);
        AddTallGrass(graph, 2f, 2.5f);

        AverageCenterPoints(graph);
    }


    private static void SetNodesToGrass(MapGraph graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            node.nodeType = MapGraph.MapNodeType.Grass;
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
                node.nodeType = MapGraph.MapNodeType.FreshWater;
            }
        }
    }

    private static void FillOcean(MapGraph graph)
    {
        var startNode = graph.nodesByCenterPosition
            .FirstOrDefault(x => x.Value.IsEdge()
                            && x.Value.nodeType == MapGraph.MapNodeType.FreshWater)
            .Value;

        FloodFill(startNode, MapGraph.MapNodeType.FreshWater, MapGraph.MapNodeType.SaltWater);
    }

    private static void FloodFill(MapGraph.MapNode node, MapGraph.MapNodeType targetType, MapGraph.MapNodeType replacementType)
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
            if (node.nodeType != MapGraph.MapNodeType.Grass)
            {
                continue;
            }

            foreach (var neighbor in node.GetNeighborNodes())
            {
                if (neighbor.nodeType == MapGraph.MapNodeType.SaltWater)
                {
                    node.nodeType = MapGraph.MapNodeType.Beach;
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
            if (node.GetElevation() > minElevation)
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

    private static void CreateRiver(MapGraph graph, MapGraph.MapNodeHalfEdge startEdge)
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

        var previousRiverEdges = new List<MapGraph.MapNodeHalfEdge>();
        do
        {
            heightUpdated = false;

            var riverEdges   = new List<MapGraph.MapNodeHalfEdge>();
            var previousEdge = startEdge;
            var nextEdge     = startEdge;

            while (nextEdge != null)
            {
                if (checkCount >= maxChecks)
                {
                    Debug.LogError("Unable to find route for river. Maximum number of checks reached");
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
                currentEdge.AddWater();

                // Check that we haven't reached the sea
                if (currentEdge.destination.GetNodes().Any(x => x.nodeType == MapGraph.MapNodeType.SaltWater))
                {
                    break;
                }

                nextEdge = GetDownSlopeEdge(currentEdge, riverEdges);

                if (nextEdge == null
                    && previousEdge != null)
                {
                    // We need to start carving a path for the river.
                    nextEdge = GetNewCandidateEdge(graph.GetCenter(), currentEdge, riverEdges, previousRiverEdges);

                    // If we can't get a candidate edge, then backtrack and try again
                    var previousEdgeIndex = riverEdges.Count - 1;
                    while (nextEdge == null
                           || previousEdgeIndex == 0)
                    {
                        previousEdge = riverEdges[previousEdgeIndex];
                        previousEdge.water--;

                        nextEdge = GetNewCandidateEdge(graph.GetCenter(), previousEdge, riverEdges, previousRiverEdges);

                        riverEdges.Remove(previousEdge);
                        previousEdgeIndex--;
                    }

                    if (nextEdge != null)
                    {
                        if (nextEdge.previous.destination.position.y != nextEdge.destination.position.y)
                        {
                            LevelEdge(nextEdge);
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

    private static MapGraph.MapNodeHalfEdge GetDownSlopeEdge(MapGraph.MapNodeHalfEdge source, List<MapGraph.MapNodeHalfEdge> seenEdges)
    {
        var corner = source.destination;

        var candidates = corner.GetEdges().Where(x =>
            x.destination.position.y < corner.position.y
            && !seenEdges.Contains(x)
            && x.opposite != null && !seenEdges.Contains(x.opposite)
            && x.node.nodeType != MapGraph.MapNodeType.FreshWater
            && x.opposite.node.nodeType != MapGraph.MapNodeType.FreshWater);

        // Make sure the river prefers to follow existing rivers
        var existingRiverEdge = candidates.FirstOrDefault(x => x.water > 0);
        if (existingRiverEdge != null)
        {
            return existingRiverEdge;
        }

        return candidates
            .OrderByDescending(x => x.GetSlopeAngle())
            .FirstOrDefault();
    }

    private static MapGraph.MapNodeHalfEdge GetNewCandidateEdge(
        Vector3 center,
        MapGraph.MapNodeHalfEdge source,
        List<MapGraph.MapNodeHalfEdge> seenEdges,
        List<MapGraph.MapNodeHalfEdge> previousEdges
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

    private static void LevelEdge(MapGraph.MapNodeHalfEdge currentEdge)
    {
        currentEdge.destination.position = new Vector3(currentEdge.destination.position.x, currentEdge.previous.destination.position.y, currentEdge.destination.position.z);
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
                node.nodeType    = MapGraph.MapNodeType.FreshWater;

                // Set all of the heights equal to where the water came in.
                SetNodeHeightToCornerHeight(node, lowestCorner);
            }
        }
    }

    private static void SetNodeHeightToCornerHeight(MapGraph.MapNode node, MapGraph.MapPoint targetCorner)
    {
        foreach (var corner in node.GetCorners())
        {
            corner.position = new Vector3(corner.position.x, targetCorner.position.y, corner.position.z);
        }

        node.centerPoint = new Vector3(node.centerPoint.x, targetCorner.position.y, node.centerPoint.z);
    }

    private static void AddMountains(
        MapGraph graph,
        float minRockyElevation, float minRockyHeightDifference,
        float minElevation, float minHeightDifference,
        float minSnowElevation
    )
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.GetElevation() > minElevation
                || node.GetHeightDifference() > minHeightDifference)
            {
                node.nodeType = MapGraph.MapNodeType.Mountain;
            } else if (node.GetElevation() > minRockyElevation
                       || node.GetHeightDifference() > minRockyHeightDifference)
            {
                node.nodeType = MapGraph.MapNodeType.Rocky;
            }

            if (node.GetElevation() > minSnowElevation)
            {
                node.nodeType = MapGraph.MapNodeType.Snow;
            }
        }
    }

    private static void AddTallGrass(MapGraph graph, float minElevation, float minHeightDifference)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            if (node.nodeType != MapGraph.MapNodeType.Grass)
            {
                continue;
            }

            if (node.GetElevation() > minElevation
                || node.GetHeightDifference() > minHeightDifference)
            {
                node.nodeType = MapGraph.MapNodeType.TallGrass;
                continue;
            }

            foreach (var neighbor in node.GetNeighborNodes())
            {
                if (neighbor.nodeType == MapGraph.MapNodeType.FreshWater)
                {
                    node.nodeType = MapGraph.MapNodeType.TallGrass;
                    break;
                }
            }
        }        
    }

    private static void AverageCenterPoints(MapGraph graph)
    {
        foreach (var node in graph.nodesByCenterPosition.Values)
        {
            node.centerPoint = new Vector3(node.centerPoint.x, node.GetCorners().Average(x => x.position.y), node.centerPoint.z);
        }
    }
}
