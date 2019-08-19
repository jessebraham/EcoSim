using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public class MapPoint
    {
        public Vector3         position;
        public MapNodeHalfEdge leavingEdge;


        public MapNodeHalfEdge GetDownSlopeEdge()
        {
            return GetEdges()
                .Where(x => x.destination.position.y <= position.y)
                .OrderBy(x => x.destination.position.y)
                .FirstOrDefault();
        }

        public IEnumerable<MapNodeHalfEdge> GetEdges()
        {
            var firstEdge = leavingEdge;
            var nextEdge  = firstEdge;

            var maxIterations = 20;
            var iterations    = 0;

            do
            {
                yield return nextEdge;
                nextEdge = nextEdge.opposite?.next;
                iterations++;
            }
            while (nextEdge != firstEdge && nextEdge != null && iterations < maxIterations);
        }

        public List<MapNode> GetNodes()
        {
            return GetEdges()
                .Select(x => x.node)
                .ToList();
        }
    }
}
