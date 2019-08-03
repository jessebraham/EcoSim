using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneratorPreview : MonoBehaviour
{
    [Header("Settings")]
    public HeightMapSettings heightMapSettings;
    public MeshSettings      terrainMeshSettings;
    public int seed = 0;

    [Header("Texture Settings")]
    public int textureSize = 512;
    public bool drawNodeBoundries;
    public bool drawDelauneyTriangles;
    public bool drawNodeCenters;
    public List<MapNodeTypeColor> colours;

    [Header("Voronoi Generation")]
    public int pointSpacing = 10;
    public int relaxationIterations = 1;
    public float snapDistance = 0;

    [Header("Outputs")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;


    public void Start()
    {
        StartCoroutine(GenerateMapAsync());
    }

    public IEnumerator GenerateMapAsync()
    {
        yield return new WaitForSeconds(1f);
        GenerateMap();
    }

    public void GenerateMap()
    {
        var startTime = DateTime.Now;
        var points    = GetPoints();

        int meshSize  = terrainMeshSettings.meshSize;

        var time    = DateTime.Now;
        var voronoi = new Delaunay.Voronoi(points, null, new Rect(0, 0, meshSize, meshSize), relaxationIterations);
        Debug.Log(string.Format("Voronoi Generated: {0:n0}ms", DateTime.Now.Subtract(time).TotalMilliseconds));

        time = DateTime.Now;
        heightMapSettings.noiseSettings.seed = seed;
        var heightMap = HeightMapGenerator.GenerateHeightMap(meshSize, meshSize, heightMapSettings, Vector2.zero);
        Debug.Log(string.Format("Heightmap Generated: {0:n0}ms", DateTime.Now.Subtract(time).TotalMilliseconds));

        time = DateTime.Now;
        var mapGraph = new MapGraph(voronoi, heightMap, snapDistance);
        Debug.Log(string.Format("Finished Generating Map Graph: {0:n0}ms with {1} nodes", DateTime.Now.Subtract(time).TotalMilliseconds, mapGraph.nodesByCenterPosition.Count));

        time = DateTime.Now;
        MapGenerator.GenerateMap(mapGraph);
        Debug.Log(string.Format("Map Generated: {0:n0}ms", DateTime.Now.Subtract(time).TotalMilliseconds));

        time = DateTime.Now;
        OnMeshDataReceived(MapMeshGenerator.GenerateMesh(mapGraph, meshSize));
        Debug.Log(string.Format("Mesh Generated: {0:n0}ms", DateTime.Now.Subtract(time).TotalMilliseconds));

        time = DateTime.Now;
        var texture = MapTextureGenerator.GenerateTexture(mapGraph, meshSize, textureSize, colours, drawNodeBoundries, drawDelauneyTriangles, drawNodeCenters);
        Debug.Log(string.Format("Texture Generated: {0:n0}ms", DateTime.Now.Subtract(time).TotalMilliseconds));

        UpdateTexture(texture);

        Debug.Log(string.Format("Finished Generating World: {0:n0}ms with {1} nodes", DateTime.Now.Subtract(startTime).TotalMilliseconds, mapGraph.nodesByCenterPosition.Count));
    }

    private List<Vector2> GetPoints()
    {
        var points = new List<Vector2>();

        for (int x = pointSpacing; x < terrainMeshSettings.meshSize; x += pointSpacing)
        {
            bool even = false;
            for (int y = pointSpacing; y < terrainMeshSettings.meshSize; y += pointSpacing)
            {
                var newX = even ? x : x - (pointSpacing / 2f);
                points.Add(new Vector2(newX, y));
                even = !even;
            }
        }

        return points;
    }

    private void OnValuesUpdated()
    {
        GenerateMap();
    }

    private void UpdateTexture(Texture2D texture)
    {
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    private void OnMeshDataReceived(object result)
    {
        UpdateMesh(result as MeshData);
    }

    public void UpdateMesh(MeshData meshData)
    {
        var mesh = new Mesh
        {
            vertices  = meshData.vertices.ToArray(),
            triangles = meshData.indices.ToArray(),
            uv        = meshData.uvs
        };

        mesh.RecalculateNormals();

        meshFilter.sharedMesh   = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
