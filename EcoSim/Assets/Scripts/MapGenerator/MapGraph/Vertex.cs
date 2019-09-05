using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public class Vertex
    {
        public Edge    leavingEdge;
        public Vector3 position;


        public Edge GetDownSlopeEdge()
        {
            return GetEdges()
                .Where(edge => edge.destination.position.y <= position.y)
                .OrderBy(edge => edge.destination.position.y)
                .FirstOrDefault();
        }

        public IEnumerable<Edge> GetEdges()
        {
            Edge firstEdge = leavingEdge;
            Edge nextEdge  = firstEdge;

            int maxIterations = 20;
            int iterations    = 0;

            do
            {
                yield return nextEdge;
                nextEdge = nextEdge.opposite?.next;
                iterations++;
            }
            while (nextEdge      != null
                   && nextEdge   != firstEdge
                   && iterations  < maxIterations);
        }
    }
}
