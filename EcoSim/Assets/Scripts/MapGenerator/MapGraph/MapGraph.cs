using Delaunay;
using Delaunay.Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public enum NodeType
    {
        SaltWater,
        FreshWater,
        Beach,
        Grass,
        TallGrass,
        Rocky,
        Mountain,
        Snow,
    }

    [Serializable]
    public class NodeTypeColour
    {
        public NodeType type;
        public Color    colour;
    }


    public Rect                        plotBounds;
    public List<Edge>                  edges;
    public Dictionary<Vector3, Vertex> vertices;
    public Dictionary<Vector3, Node>   nodesByCenterPosition;

    public Vector3 center
    {
        get
        {
            return ToVector3(plotBounds.center);
        }
    }


    public MapGraph(Voronoi voronoi, HeightMap heightMap, float snapDistance)
    {     
        CreateFromVoronoi(voronoi);

        if (snapDistance > 0f)
        {
            SnapPoints(snapDistance);
        }

        UpdateHeights(heightMap);
    }


    public IEnumerable<Node> FilterNodes(IEnumerable<NodeType> types)
    {
        return nodesByCenterPosition.Values
            .Where(node => types.Contains(node.nodeType));
    }

    public IEnumerable<Node> FilterNodes(NodeType type)
    {
        return nodesByCenterPosition.Values
            .Where(node => node.nodeType == type);
    }


    private void CreateFromVoronoi(Voronoi voronoi)
    {
        vertices = new Dictionary<Vector3, Vertex>();

        nodesByCenterPosition    = new Dictionary<Vector3, Node>();
        var edgesByStartPosition = new Dictionary<Vector3, List<Edge>>();

        edges = new List<Edge>();

        plotBounds = voronoi.plotBounds;

        var bottomLeftSite  = voronoi.NearestSitePoint(plotBounds.xMin, plotBounds.yMin);
        var bottomRightSite = voronoi.NearestSitePoint(plotBounds.xMax, plotBounds.yMin);
        var topLeftSite     = voronoi.NearestSitePoint(plotBounds.xMin, plotBounds.yMax);
        var topRightSite    = voronoi.NearestSitePoint(plotBounds.xMax, plotBounds.yMax);

        var topLeft     = new Vector3(plotBounds.xMin, 0, plotBounds.yMax);
        var topRight    = new Vector3(plotBounds.xMax, 0, plotBounds.yMax);
        var bottomLeft  = new Vector3(plotBounds.xMin, 0, plotBounds.yMin);
        var bottomRight = new Vector3(plotBounds.xMax, 0, plotBounds.yMin);

        var siteEdges = new Dictionary<Vector2, List<LineSegment>>();

        var edgePointsRemoved = 0;

        foreach (var edge in voronoi.Edges())
        {
            if (!edge.visible)
            {
                continue;
            }

            var p1 = edge.clippedEnds[Delaunay.LR.Side.LEFT];
            var p2 = edge.clippedEnds[Delaunay.LR.Side.RIGHT];

            var segment = new LineSegment(p1, p2);

            if (Vector2.Distance(p1.Value, p2.Value) < 0.001f)
            {
                edgePointsRemoved++;
                continue;
            }

            if (edge.leftSite != null)
            {
                if (!siteEdges.ContainsKey(edge.leftSite.Coord))
                {
                    siteEdges.Add(edge.leftSite.Coord, new List<LineSegment>());
                }
                siteEdges[edge.leftSite.Coord].Add(segment);
            }

            if (edge.rightSite != null)
            {
                if (!siteEdges.ContainsKey(edge.rightSite.Coord))
                {
                    siteEdges.Add(edge.rightSite.Coord, new List<LineSegment>());
                }
                siteEdges[edge.rightSite.Coord].Add(segment);
            }
        }

        Debug.Assert(edgePointsRemoved == 0, string.Format("{0} edge points too close and have been removed", edgePointsRemoved));

        foreach (var site in voronoi.SiteCoords())
        {
            var boundries   = GetBoundriesForSite(siteEdges, site);
            var center      = ToVector3(site);
            var currentNode = new Node { centerPoint = center };

            nodesByCenterPosition.Add(center, currentNode);

            Edge firstEdge    = null;
            Edge previousEdge = null;

            for (var i = 0; i < boundries.Count; i++)
            {
                var edge = boundries[i];

                var start = ToVector3(edge.p0.Value);
                var end   = ToVector3(edge.p1.Value);
                if (start == end)
                {
                    continue;
                }

                previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, end, currentNode);
                if (firstEdge == null)
                {
                    firstEdge = previousEdge;
                }
                if (currentNode.startEdge == null)
                {
                    currentNode.startEdge = previousEdge;
                }

                // We need to figure out if the two edges meet, and if not then
                // insert some more edges to close the polygon
                var insertEdges = false;
                if (i < boundries.Count - 1)
                {
                    start       = ToVector3(boundries[i + 0].p1.Value);
                    end         = ToVector3(boundries[i + 1].p0.Value);
                    insertEdges = start != end;
                }
                else if (i == boundries.Count - 1)
                {
                    start       = ToVector3(boundries[i].p1.Value);
                    end         = ToVector3(boundries[0].p0.Value);
                    insertEdges = start != end;
                }

                if (insertEdges)
                {
                    // Check which corners are within this node
                    var startIsTop    = start.z == voronoi.plotBounds.yMax;
                    var startIsBottom = start.z == voronoi.plotBounds.yMin;
                    var startIsLeft   = start.x == voronoi.plotBounds.xMin;
                    var startIsRight  = start.x == voronoi.plotBounds.xMax;

                    var hasTopLeft     = site == topLeftSite && !(startIsTop && startIsLeft);
                    var hasTopRight    = site == topRightSite && !(startIsTop && startIsRight);
                    var hasBottomLeft  = site == bottomLeftSite && !(startIsBottom && startIsLeft);
                    var hasBottomRight = site == bottomRightSite && !(startIsBottom && startIsRight);

                    if (startIsTop)
                    {
                        if (hasTopRight)    previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, topRight, currentNode);
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomRight, currentNode);
                        if (hasBottomLeft)  previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomLeft, currentNode);
                        if (hasTopLeft)     previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topLeft, currentNode);
                    }
                    else if (startIsRight)
                    {
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, bottomRight, currentNode);
                        if (hasBottomLeft)  previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomLeft, currentNode);
                        if (hasTopLeft)     previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topLeft, currentNode);
                        if (hasTopRight)    previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topRight, currentNode);
                    }
                    else if (startIsBottom)
                    {
                        if (hasBottomLeft)  previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, bottomLeft, currentNode);
                        if (hasTopLeft)     previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topLeft, currentNode);
                        if (hasTopRight)    previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topRight, currentNode);
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomRight, currentNode);
                    }
                    else if (startIsLeft)
                    {
                        if (hasTopLeft)     previousEdge = AddEdge(edgesByStartPosition, previousEdge, start, topLeft, currentNode);
                        if (hasTopRight)    previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, topRight, currentNode);
                        if (hasBottomRight) previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomRight, currentNode);
                        if (hasBottomLeft)  previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, bottomLeft, currentNode);
                    }

                    previousEdge = AddEdge(edgesByStartPosition, previousEdge, previousEdge.destination.position, end, currentNode);
                }
            }

            // Connect up the end of the loop
            previousEdge.next  = firstEdge;
            firstEdge.previous = previousEdge;

            AddLeavingEdge(firstEdge);
        }

        ConnectOpposites(edgesByStartPosition);
    }

    private void SnapPoints(float snapDistance)
    {
        var keys = vertices.Keys.ToList();

        foreach (var key in keys)
        {
            // We have to check to see if it hasn't been deleted by an earlier snap
            if (!vertices.ContainsKey(key))
            {
                continue;
            }

            var vertex    = vertices[key];
            var neighbors = vertex.GetEdges();

            foreach (var neighbor in neighbors)
            {
                if (snapDistance > Vector3.Distance(vertex.position, neighbor.destination.position))
                {
                    SnapPoints(vertex, neighbor);
                }
            }
        }
    }

    private void SnapPoints(Vertex point, Edge edge)
    {
        // Don't snap if the neighboring nodes already have three edges
        if (edge.node.GetEdges().Count() <= 3
            || edge.opposite == null
            || edge.opposite.node.GetEdges().Count() <= 3)
        {
            return;
        }

        // There are issues with this when snapping near the edge of the map
        if (point.GetEdges().Any(x => x.opposite == null) 
            || edge.destination.GetEdges().Any(x => x.opposite == null))
        {
            return;
        }

        // Delete the edges
        edges.Remove(edge);

        // Delete the other point
        vertices.Remove(new Vector3(edge.destination.position.x, 0, edge.destination.position.z));

        var otherEdges = edge.destination.GetEdges().ToList();

        // Update everything to point to the first point
        if (point.leavingEdge == edge)
        {
            point.leavingEdge = edge.opposite.next;
        }

        if (edge.node.startEdge == edge)
        {
            edge.node.startEdge = edge.previous;
        }

        edge.next.previous = edge.previous;
        edge.previous.next = edge.next;

        // Update the opposite edge as well
        if (edge.opposite?.node.GetEdges().Count() > 3)
        {
            // Delete edge
            edges.Remove(edge.opposite);

            // Update pointers
            edge.opposite.next.previous = edge.opposite.previous;
            edge.opposite.previous.next = edge.opposite.next;

            if (edge.opposite.node.startEdge == edge.opposite)
            {
                edge.opposite.node.startEdge = edge.opposite.previous;
            }
        }

        foreach (var otherEdge in otherEdges)
        {
            if (otherEdge.opposite != null)
            {
                otherEdge.opposite.destination = point;
            }
        }
    }

    private void UpdateHeights(HeightMap heightmap)
    {
        foreach (var node in nodesByCenterPosition.Values)
        {
            node.centerPoint = UpdateHeight(heightmap, node.centerPoint);
        }

        foreach (var point in vertices.Values)
        {
            point.position = UpdateHeight(heightmap, point.position);
        }
    }

    private static void AddLeavingEdge(Edge edge)
    {
        if (edge.previous.destination.leavingEdge == null)
        {
            edge.previous.destination.leavingEdge = edge;
        }
    }

    private static Vector3 UpdateHeight(HeightMap heightmap, Vector3 oldPosition)
    {
        var position = oldPosition;

        var x = Mathf.FloorToInt(position.x);
        var y = Mathf.FloorToInt(position.z);

        if (x >= 0
            && y >= 0
            && x < heightmap.values.GetLength(0)
            && y < heightmap.values.GetLength(1))
        {
            position.y = heightmap.values[x, y];
        }

        return position;
    }

    private List<LineSegment> GetBoundriesForSite(Dictionary<Vector2, List<LineSegment>> siteEdges, Vector2 site)
    {
        var boundries = siteEdges[site];

        // Sort boundries clockwise
        boundries = FlipClockwise(boundries, site);
        boundries = SortClockwise(boundries, site);
        boundries = SnapBoundries(boundries, 0.001f);

        return boundries;
    }

    private static List<LineSegment> SnapBoundries(List<LineSegment> boundries, float snapDistance)
    {
        for (int i = boundries.Count - 1; i >= 0; i--)
        {
            if (Vector2.Distance(boundries[i].p0.Value, boundries[i].p1.Value) < snapDistance)
            {
                var previous = i - 1;
                var next     = i + 1;

                if (previous < 0)
                {
                    previous = boundries.Count - 1;
                }

                if (next >= boundries.Count)
                {
                    next = 0;
                }

                if (Vector2.Distance(boundries[previous].p1.Value, boundries[next].p0.Value) < snapDistance)
                {
                    boundries[previous].p1 = boundries[next].p0;
                }

                boundries.Remove(boundries[i]);
            }
        }

        return boundries;
    }

    private void ConnectOpposites(Dictionary<Vector3, List<Edge>> edgesByStartPosition)
    {
        foreach (var edge in edges)
        {
            if (edge.opposite != null)
            {
                continue;
            }

            var startEdgePosition = edge.previous.destination.position;
            var endEdgePosition   = edge.destination.position;

            if (edgesByStartPosition.ContainsKey(endEdgePosition))
            {
                var list = edgesByStartPosition[endEdgePosition];

                Edge opposite = null;
                foreach (var item in list)
                {
                    // We use 0.5f to snap the coordinates to each other,
                    // otherwise there are holes in the graph.
                    if (Math.Abs(item.destination.position.x - startEdgePosition.x) < 0.5f
                        && Math.Abs(item.destination.position.z - startEdgePosition.z) < 0.5f)
                    {
                        opposite = item;
                    }
                }

                if (opposite != null)
                {
                    edge.opposite     = opposite;
                    opposite.opposite = edge;
                }
            }
        }
    }

    private List<LineSegment> SortClockwise(List<LineSegment> segments, Vector2 center)
    {
        segments.Sort((line1, line2) =>
        {
            var firstVector  = line1.p0.Value - center;
            var secondVector = line2.p0.Value - center;

            var angle = Vector2.SignedAngle(firstVector, secondVector);

            return angle > 0 ? 1 : (angle < 0 ? -1 : 0);
        });

        return segments;
    }

    private List<LineSegment> FlipClockwise(List<LineSegment> segments, Vector2 center)
    {
        var newSegments = new List<LineSegment>();
        foreach (var line in segments)
        {
            var firstVector  = line.p0.Value - center;
            var secondVector = line.p1.Value - center;

            var angle = Vector2.SignedAngle(firstVector, secondVector);

            if (angle > 0)
            {
                newSegments.Add(new LineSegment(line.p1, line.p0));
            }
            else
            {
                newSegments.Add(new LineSegment(line.p0, line.p1));
            }
        }

        return newSegments;
    }

    private Edge AddEdge(
        Dictionary<Vector3, List<Edge>> edgesByStartPosition,
        Edge previous,
        Vector3 start,
        Vector3 end,
        Node node
    )
    {
        if (start == end)
        {
            Debug.Assert(start != end, "Start and end vectors must not be the same");
        }

        var currentEdge = new Edge { node = node };

        if (!vertices.ContainsKey(start))
        {
            vertices.Add(start, new Vertex { position = start, leavingEdge = currentEdge });
        }

        if (!vertices.ContainsKey(end))
        {
            vertices.Add(end, new Vertex { position = end });
        }

        currentEdge.destination = vertices[end];

        if (!edgesByStartPosition.ContainsKey(start))
        {
            edgesByStartPosition.Add(start, new List<Edge>());
        }

        edgesByStartPosition[start].Add(currentEdge);
        edges.Add(currentEdge);

        if (previous != null)
        {
            previous.next        = currentEdge;
            currentEdge.previous = previous;

            AddLeavingEdge(currentEdge);
        }

        return currentEdge;
    }

    private Vector3 ToVector3(Vector2 vector)
    {
        return new Vector3(Mathf.Round(vector.x * 1000f) / 1000f, 0, Mathf.Round(vector.y * 1000f) / 1000f);
    }
}
