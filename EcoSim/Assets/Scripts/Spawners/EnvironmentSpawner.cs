using System.Linq;
using UnityEngine;

public static class EnvironmentSpawner
{
    // Trees will only spawn on Grass and TallGrass type nodes.
    private static MapGraph.MapNodeType[] treeNodeTypes =
    {
        MapGraph.MapNodeType.Grass,
        MapGraph.MapNodeType.TallGrass,
    };

    // Rocks will only spawn on Grass, TallGrass, or Rocky type nodes.
    private static MapGraph.MapNodeType[] rockNodeTypes =
    {
        MapGraph.MapNodeType.Grass,
        MapGraph.MapNodeType.TallGrass,
        MapGraph.MapNodeType.Rocky,
    };

    private static EnvironmentSettings settings;
    private static PrefabSpawner       spawner;


    public static void Spawn(EnvironmentSettings settings, Transform container, MapGraph mapGraph, int seed)
    {
        EnvironmentSpawner.settings = settings;
        EnvironmentSpawner.spawner  = new PrefabSpawner(container, mapGraph, seed);

        SpawnTrees();
        SpawnRocks();
    }


    static void SpawnTrees()
    {
        foreach (var node in spawner.FilterNodes(treeNodeTypes))
        {
            float probability;
            MeshRenderer treePrefab;

            // For now, Deciduous trees spawn on Grass and Coniferous trees
            // spawn on TallGrass.
            if (node.nodeType == MapGraph.MapNodeType.Grass)
            {
                probability = settings.deciduousProbability;
                treePrefab  = settings.deciduousTreePrefab;
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

                probability = settings.coniferousProbability;
                treePrefab  = settings.coniferousTreePrefab;
            }

            if (spawner.ShouldSpawn(probability))
            {
                MeshRenderer obj = spawner.SpawnPrefab(node, treePrefab, settings.treeScale, settings.treeScaleDeviation, RandomRotation());
                obj.transform.gameObject.AddComponent<MeshCollider>();
            }
        }
    }

    static void SpawnRocks()
    {
        foreach (var node in spawner.FilterNodes(rockNodeTypes))
        {
            if (spawner.ShouldSpawn(settings.rockProbability))
            {
                MeshRenderer obj = spawner.SpawnPrefab(node, settings.rockPrefab, settings.rockScale, settings.rockScaleDeviation, RandomRotation());
                obj.transform.gameObject.AddComponent<MeshCollider>();
            }
        }
    }

    static Quaternion RandomRotation()
    {
        // X and Z axis rotations are randomized within the bounds of the
        // maxRotation setting.
        float x = Mathf.Lerp(-settings.maxRotation, settings.maxRotation, (float)spawner.prng.NextDouble());
        float z = Mathf.Lerp(-settings.maxRotation, settings.maxRotation, (float)spawner.prng.NextDouble());

        // Y axis rotation can be any valid angle.
        float y = (float)spawner.prng.NextDouble() * 360f;

        return Quaternion.Euler(x, y, z);
    }
}
