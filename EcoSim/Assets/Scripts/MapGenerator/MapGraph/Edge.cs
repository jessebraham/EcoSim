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
    }
}
