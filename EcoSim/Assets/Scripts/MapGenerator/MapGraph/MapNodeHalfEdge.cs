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

        public override string ToString()
        {
            return "HalfEdge: " + previous.destination.position  + " -> " + destination.position;
        }
    }
}
