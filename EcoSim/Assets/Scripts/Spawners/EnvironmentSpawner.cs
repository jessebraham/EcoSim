using System.Linq;
using UnityEngine;

public static class EnvironmentSpawner
{
    // Trees will only spawn on Grass and TallGrass type nodes.
    private static MapGraph.NodeType[] treeNodeTypes =
    {
        MapGraph.NodeType.Grass,
        MapGraph.NodeType.TallGrass,
    };

    // Rocks will only spawn on Grass, TallGrass, or Rocky type nodes.
    private static MapGraph.NodeType[] rockNodeTypes =
    {
        MapGraph.NodeType.Grass,
        MapGraph.NodeType.TallGrass,
        MapGraph.NodeType.Rocky,
    };

    private static MapGraph            mapGraph;
    private static System.Random       prng;
    private static EnvironmentSettings settings;


    public static void Spawn(MapGraph mapGraph, Transform container, EnvironmentSettings settings, int seed)
    {
        EnvironmentSpawner.mapGraph = mapGraph;
        EnvironmentSpawner.prng     = new System.Random(seed);
        EnvironmentSpawner.settings = settings;

        var spawner = new PrefabSpawner(container);
        SpawnTrees(spawner);
        SpawnRocks(spawner);
    }


    static void SpawnTrees(PrefabSpawner spawner)
    {
        foreach (var node in mapGraph.FilterNodes(treeNodeTypes))
        {
            float probability;
            MeshRenderer treePrefab;

            // For now, Deciduous trees spawn on Grass and Coniferous trees
            // spawn on TallGrass.
            if (node.nodeType == MapGraph.NodeType.Grass)
            {
                probability = settings.deciduousProbability;
                treePrefab  = settings.deciduousTreePrefab;
            }
            else
            {
                // Don't spawn coniferous trees beside water, or if none of the
                // neighbouring nodes are of the same type, because I say so.
                if (node.GetNeighborNodes().Any(neighbour => neighbour.nodeType == MapGraph.NodeType.FreshWater)
                    || !node.GetNeighborNodes().Any(neighbour => neighbour.nodeType == MapGraph.NodeType.TallGrass))
                {
                    continue;
                }

                probability = settings.coniferousProbability;
                treePrefab  = settings.coniferousTreePrefab;
            }

            if (prng.NextDouble() < probability)
            {
                MeshRenderer obj = spawner.SpawnPrefab(
                    node.centerPoint - mapGraph.center,
                    treePrefab,
                    ScaleModifier(settings.treeScale, settings.treeScaleDeviation),
                    RandomRotation()
                );
                obj.transform.gameObject.AddComponent<MeshCollider>();

                // Mark the node as occupied.
                node.occupiedByEnvironment = true;
            }
        }
    }

    static void SpawnRocks(PrefabSpawner spawner)
    {
        foreach (var node in mapGraph.FilterNodes(rockNodeTypes))
        {
            if (prng.NextDouble() < settings.rockProbability)
            {
                MeshRenderer obj = spawner.SpawnPrefab(
                    node.centerPoint - mapGraph.center,
                    settings.rockPrefab,
                    ScaleModifier(settings.rockScale, settings.rockScaleDeviation),
                    RandomRotation()
                );
                obj.transform.gameObject.AddComponent<MeshCollider>();

                // Mark the node as occupied.
                node.occupiedByEnvironment = true;
            }
        }
    }

    static float ScaleModifier(float scale, float scaleDeviation)
    {
        return scale + Mathf.Lerp(0, scaleDeviation, (float)prng.NextDouble());
    }

    static Quaternion RandomRotation()
    {
        // X and Z axis rotations are randomized within the bounds of the
        // maxRotation setting.
        float x = Mathf.Lerp(-settings.maxRotation, settings.maxRotation, (float)prng.NextDouble());
        float z = Mathf.Lerp(-settings.maxRotation, settings.maxRotation, (float)prng.NextDouble());

        // Y axis rotation can be any valid angle.
        float y = (float)prng.NextDouble() * 360f;

        return Quaternion.Euler(x, y, z);
    }
}
