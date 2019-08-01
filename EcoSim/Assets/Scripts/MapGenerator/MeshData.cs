using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices;
    public List<int>     indices;
    public Vector2[]     uvs;


    public MeshData()
    {
        vertices = new List<Vector3>();
        indices  = new List<int>();
    }

    public void AddTriangle(int a, int b, int c)
    {
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }
}
