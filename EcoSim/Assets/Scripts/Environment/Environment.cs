using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Environment
{
    private static readonly float maxRotation = 4f;

    private static Transform container;
    private static MapGraph mapGraph;
    private static Vector3 mapCenter;
    private static System.Random prng;


    public static void Init(MapGraph mapGraph, Transform container, int seed)
    {
        Environment.mapGraph  = mapGraph;
        Environment.container = container;

        mapCenter = mapGraph.GetCenter();
        prng      = new System.Random(seed);

        ClearGameObjects();
    }

    public static void SpawnTrees(
        MeshRenderer coniferousPrefab,
        MeshRenderer deciduousPrefab,
        float coniferousProbability,
        float deciduousProbability,
        float scale,
        float scaleDeviation
    )
    {
        // Trees will only spawn on grass type nodes.
        var nodeTypes = new MapGraph.MapNodeType[]
        {
            MapGraph.MapNodeType.Grass,
            MapGraph.MapNodeType.TallGrass,
        };

        foreach (var node in FilterNodes(nodeTypes))
        {
            // If the node is marked as occupied, we won't spawn another object
            // on it.
            if (node.occupied)
            {
                continue;
            }

            float probability;
            MeshRenderer treePrefab;

            // For now, Deciduous trees spawn on Grass and Coniferous trees
            // spawn on TallGrass.
            if (node.nodeType == MapGraph.MapNodeType.Grass)
            {
                probability = deciduousProbability;
                treePrefab  = deciduousPrefab;
            }
            else
            {
                // Don't spawn coniferous trees beside water, or if none of the
                // neighbouring nodes are of the same type, because I say so.
                if (node.GetNeighborNodes().Any(neighbour => neighbour.nodeType == MapGraph.MapNodeType.FreshWater)
                    || !node.GetNeighborNodes().Any(neighbour => neighbour.nodeType == MapGraph.MapNodeType.TallGrass))
                {
                    continue;
                }

                probability = coniferousProbability;
                treePrefab  = coniferousPrefab;
            }

            if (prng.NextDouble() < probability)
            {
                SpawnPrefab(node, treePrefab, scale, scaleDeviation);
            }
        }
    }

    public static void SpawnRocks(
        MeshRenderer rockPrefab,
        float probability,
        float scale,
        float scaleDeviation
    )
    {
        // Trees will only spawn on grass or rocky type nodes.
        var nodeTypes = new MapGraph.MapNodeType[]
        {
            MapGraph.MapNodeType.Grass,
            MapGraph.MapNodeType.TallGrass,
            MapGraph.MapNodeType.Rocky,
        };

        foreach (var node in FilterNodes(nodeTypes))
        {
            // If the node is marked as occupied, we won't spawn another object
            // on it.
            if (node.occupied)
            {
                continue;
            }

            if (prng.NextDouble() < probability)
            {
                SpawnPrefab(node, rockPrefab, scale, scaleDeviation);
            }
        }
    }


    static void ClearGameObjects()
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in container)
        {
            children.Add(child.gameObject);
        }

        foreach (var child in children)
        {
            GameObject.DestroyImmediate(child);
        }
    }

    static List<MapGraph.MapNode> FilterNodes(MapGraph.MapNodeType[] types)
    {
        return mapGraph.nodesByCenterPosition.Values
            .Where(node => types.Contains(node.nodeType))
            .ToList();
    }

    static void SpawnPrefab(MapGraph.MapNode node, MeshRenderer prefab, float scale, float deviation)
    {
        MeshRenderer obj = Object.Instantiate(prefab, node.centerPoint - mapCenter, randomizedRotation()) as MeshRenderer;
        obj.transform.localScale *= scale + Mathf.Lerp(0, deviation, (float)prng.NextDouble());
        obj.transform.parent = container;

        // Mark the node as occupied.
        node.occupied = true;
    }

    static Quaternion randomizedRotation()
    {
        float x = Mathf.Lerp(-maxRotation, maxRotation, (float)prng.NextDouble());
        float y = (float)prng.NextDouble() * 360f;
        float z = Mathf.Lerp(-maxRotation, maxRotation, (float)prng.NextDouble());

        return Quaternion.Euler(x, y, z);
    }
}
