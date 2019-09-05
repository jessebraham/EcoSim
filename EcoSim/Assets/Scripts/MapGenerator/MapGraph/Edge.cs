using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public class Edge
    {
        public Vertex destination;

        public Edge opposite;
        public Edge previous;
        public Edge next;

        public Node node;

        public int water;

        public Vector3 startPosition
        {
            get
            {
                return previous.destination.position;
            }
        }
        public Vector3 endPosition
        {
            get
            {
                return destination.position;
            }
        }


        public Edge GetDownSlopeEdge(IEnumerable<Edge> seenEdges)
        {
            var candidates = destination.GetEdges()
                .Where(edge => edge.opposite != null
                               && !seenEdges.Contains(edge)
                               && !seenEdges.Contains(edge.opposite)
                               && edge.destination.position.y  < destination.position.y
                               && edge.node.nodeType          != MapGraph.NodeType.FreshWater
                               && edge.opposite.node.nodeType != MapGraph.NodeType.FreshWater);

            // Make sure the river prefers to follow existing rivers
            var existingRiverEdge = candidates.FirstOrDefault(edge => edge.water > 0);
            if (existingRiverEdge != null)
            {
                return existingRiverEdge;
            }

            return candidates
                .OrderByDescending(edge => edge.GetSlopeAngle())
                .FirstOrDefault();
        }


        private float GetSlopeAngle()
        {
            var vector    = destination.position - previous.destination.position;
            var direction = new Vector3(vector.x, 0f, vector.z);

            return Vector3.Angle(direction, vector);
        }
    }
}
