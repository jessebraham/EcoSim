using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public class MapNode
    {
        private float? heightDifference;

        public Vector3         centerPoint;
        public MapNodeHalfEdge startEdge;

        public MapNodeType nodeType;

        public bool occupied = false;


        public IEnumerable<MapNodeHalfEdge> GetEdges()
        {
            yield return startEdge;

            var next = startEdge.next;
            while(next != startEdge)
            {
                yield return next;
                next = next.next;
            }
        }

        public IEnumerable<MapPoint> GetCorners()
        {
            yield return startEdge.destination;

            var next = startEdge.next;
            while (next != startEdge)
            {
                yield return next.destination;
                next = next.next;
            }
        }

        public bool IsEdge()
        {
            foreach (var edge in GetEdges())
            {
                if (edge.opposite == null)
                {
                    return true;
                }
            }

            return false;
        }

        public float GetElevation()
        {
            return centerPoint.y;
        }

        public float GetHeightDifference()
        {
            if (!heightDifference.HasValue)
            {
                var lowestY  = centerPoint.y;
                var highestY = centerPoint.y;

                foreach(var corner in GetCorners())
                {
                    if (corner.position.y > highestY)
                    {
                        highestY = corner.position.y;
                    }

                    if (corner.position.y < lowestY)
                    {
                        lowestY = corner.position.y;
                    }
                }

                heightDifference = highestY - lowestY;
            }

            return heightDifference.Value;
        }

        internal MapPoint GetLowestCorner()
        {
            return GetCorners()
                .OrderBy(x => x.position.y)
                .FirstOrDefault();
        }

        public List<MapNode> GetNeighborNodes()
        {
            return GetEdges()
                .Where(x => x.opposite != null && x.opposite.node != null).Select(x => x.opposite.node)
                .ToList();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", base.ToString(), centerPoint);
        }
    }
}
