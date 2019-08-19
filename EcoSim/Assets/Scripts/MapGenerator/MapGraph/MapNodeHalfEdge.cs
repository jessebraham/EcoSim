using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public class MapNodeHalfEdge
    {
        public MapPoint destination;

        public MapNodeHalfEdge next;
        public MapNodeHalfEdge previous;
        public MapNodeHalfEdge opposite;

        public MapNode node;

        internal int water;


        public Vector3 GetStartPosition()
        {
            return previous.destination.position;
        }

        public Vector3 GetEndPosition()
        {
            return destination.position;
        }

        public void AddWater()
        {
            water++;
        }

        public float GetSlopeAngle()
        {
            var vector    = destination.position - previous.destination.position;
            var direction = new Vector3(vector.x, 0f, vector.z);
            var angle     = Vector3.Angle(direction, vector);

            return angle;
        }

        public MapGraph.MapNodeHalfEdge GetDownSlopeEdge(List<MapGraph.MapNodeHalfEdge> seenEdges)
        {
            var candidates = destination.GetEdges().Where(x =>
                x.destination.position.y < destination.position.y
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
    }
}
