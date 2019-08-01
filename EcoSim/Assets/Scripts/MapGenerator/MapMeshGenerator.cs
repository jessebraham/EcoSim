using System.Linq;
using UnityEngine;

public static class MapMeshGenerator
{
    public static MeshData GenerateMesh(MapGraph mapGraph, int meshSize)
    {
        var meshData = new MeshData();

        foreach (var node in mapGraph.nodesByCenterPosition.Values)
        {
            meshData.vertices.Add(node.centerPoint);

            var centerIndex = meshData.vertices.Count - 1;
            var edges       = node.GetEdges().ToList();

            int lastIndex  = 0;
            int firstIndex = 0;

            for (var i = 0; i < edges.Count(); i++)
            {
                if (i == 0)
                {
                    meshData.vertices.Add(edges[i].previous.destination.position);
                    var j = meshData.vertices.Count - 1;

                    meshData.vertices.Add(edges[i].destination.position);
                    var k = meshData.vertices.Count - 1;

                    meshData.AddTriangle(centerIndex, j, k);

                    firstIndex = j;
                    lastIndex  = k;
                }
                else if (i < edges.Count() -1)
                {
                    meshData.vertices.Add(edges[i].destination.position);
                    var currentIndex = meshData.vertices.Count - 1;

                    meshData.AddTriangle(centerIndex, lastIndex, currentIndex);

                    lastIndex = currentIndex;
                } 
                else
                {
                    meshData.AddTriangle(centerIndex, lastIndex, firstIndex);
                }
            }
        }

        meshData.uvs = new Vector2[meshData.vertices.Count];
        for (int i = 0; i < meshData.uvs.Length; i++)
        {
            meshData.uvs[i] = new Vector2(meshData.vertices[i].x / meshSize, meshData.vertices[i].z / meshSize);
        }

        return meshData;
    }
}
