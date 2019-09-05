using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapGraph
{
    public class Node
    {
        public Vector3  centerPoint;
        public NodeType nodeType;
        public Edge     startEdge;

        public float elevation
        {
            get
            {
                return centerPoint.y;
            }
        }

        public bool occupiedByAnimal      = false;
        public bool occupiedByEnvironment = false;
        public bool occupied
        {
            get
            {
                return occupiedByAnimal || occupiedByEnvironment;
            }
        }


        private float? heightDifference;


        public IEnumerable<Edge> GetEdges()
        {
            yield return startEdge;

            var next = startEdge.next;
            while(next != startEdge)
            {
                yield return next;
                next = next.next;
            }
        }

        public IEnumerable<Node> GetNeighborNodes()
        {
            return GetEdges()
                .Where(edge => edge.opposite?.node != null)
                .Select(edge => edge.opposite.node);
        }

        public IEnumerable<Vertex> GetCorners()
        {
            yield return startEdge.destination;

            var next = startEdge.next;
            while (next != startEdge)
            {
                yield return next.destination;
                next = next.next;
            }
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

        public Vertex GetLowestCorner()
        {
            return GetCorners()
                .OrderBy(point => point.position.y)
                .FirstOrDefault();
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

        public void SetNodeHeightToCornerHeight(Vertex targetCorner)
        {
            foreach (var corner in GetCorners())
            {
                corner.position = new Vector3(corner.position.x, targetCorner.position.y, corner.position.z);
            }

            centerPoint = new Vector3(centerPoint.x, targetCorner.position.y, centerPoint.z);
        }
    }
}
