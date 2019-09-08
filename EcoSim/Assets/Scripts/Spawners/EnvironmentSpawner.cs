using System.Linq;
using UnityEngine;

public static class EnvironmentSpawner
{
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
        var tallGrassNodes = mapGraph.FilterNodes(MapGraph.NodeType.TallGrass).ToList();
        int numConiferous  = (int)(tallGrassNodes.Count * settings.coniferousAvgDensity);

        var grassNodes   = mapGraph.FilterNodes(MapGraph.NodeType.Grass).ToList();
        int numDeciduous = (int)(grassNodes.Count * settings.deciduousAvgDensity);

        // Spawn coniferous trees.
        for (int i = 0; i < numConiferous; i++)
        {
            // Randomly select a node. If it has more than the maximum number
            // of occupants, or any of the neighbouring nodes are FreshWater,
            // try again.
            var node = tallGrassNodes.ElementAt(prng.Next(tallGrassNodes.Count));
            if (node.numEnvironmentOccupants >= settings.maxOccupantsPerNode
                || node.GetNeighbourNodes().Any(neighbour => neighbour.nodeType == MapGraph.NodeType.FreshWater))
            {
                i--;
                continue;
            }

            SpawnTreePrefab(spawner, node, settings.coniferousTreePrefab, settings.coniferousTreeScale);
        }

        // Spawn deciduous trees.
        for (int i = 0; i < numDeciduous; i++)
        {
            // Randomly select a node. If it has more than the maximum number
            // of occupants, try again.
            var node = grassNodes.ElementAt(prng.Next(grassNodes.Count));
            if (node.numEnvironmentOccupants >= settings.maxOccupantsPerNode)
            {
                i--;
                continue;
            }

            SpawnTreePrefab(spawner, node, settings.deciduousTreePrefab, settings.deciduousTreeScale);
        }
    }

    static void SpawnTreePrefab(PrefabSpawner spawner, MapGraph.Node node, MeshRenderer prefab, float scale)
    {
        MeshRenderer obj = spawner.SpawnPrefab(
            prefab,
            RandomOffset(node.centerPoint, mapGraph.center, settings.treePositionDeviation),
            RandomRotation(settings.treeRotationDeviation),
            RandomScaling(scale, settings.treeScaleDeviation)
        );
        obj.transform.gameObject.AddComponent<MeshCollider>();

        // Increment the number of environment occupants for the
        // current node.
        node.numEnvironmentOccupants++;
    }

    static void SpawnRocks(PrefabSpawner spawner)
    {
        var allowedNodeTypes = new MapGraph.NodeType[] {
            MapGraph.NodeType.Grass,
            MapGraph.NodeType.TallGrass,
            MapGraph.NodeType.Rocky
        };

        var rockNodes = mapGraph.FilterNodes(allowedNodeTypes).ToList();
        int numRocks  = (int)(rockNodes.Count * settings.rockAvgDensity);

        for (int i = 0; i < numRocks; i++)
        {
            // Randomly select a node. If it has more than the maximum number
            // of occupants, try again.
            var node = rockNodes.ElementAt(prng.Next(rockNodes.Count));
            if (node.numEnvironmentOccupants > 0)
            {
                i--;
                continue;
            }

            MeshRenderer obj = spawner.SpawnPrefab(
                settings.rockPrefab,
                RandomOffset(node.centerPoint, mapGraph.center, settings.rockPositionDeviation),
                RandomRotation(settings.rockRotationDeviation),
                RandomScaling(settings.rockScale, settings.rockScaleDeviation)
            );
            obj.transform.gameObject.AddComponent<MeshCollider>();

            // Increment the number of environmental occupants for the
            // current node.
            node.numEnvironmentOccupants++;
        }
    }

    static Vector3 RandomOffset(Vector3 nodeCenter, Vector3 mapCenter, float maxDeviation)
    {
        // Subtract the map center point from the node center to get the proper
        // global coordinates, and apply a random offset to this position's X
        // and Z axes.
        float x = Mathf.Lerp(-maxDeviation, maxDeviation, (float)prng.NextDouble()) / 2f;
        float z = Mathf.Lerp(-maxDeviation, maxDeviation, (float)prng.NextDouble()) / 2f;

        return nodeCenter - mapCenter + new Vector3(x, 0f, z);
    }

    static float RandomScaling(float scale, float maxDeviation)
    {
        // Apply a random deviation to the provided scale. Deviation can be
        // positive or negative, making the default `scale` the median.
        float deviation = Mathf.Lerp(-maxDeviation, maxDeviation, (float)prng.NextDouble()) / 2f;
        return scale + deviation;
    }

    static Quaternion RandomRotation(float maxRotation)
    {
        // X and Z axis rotations are randomized within the bounds of
        // `maxRotation`.
        float x = Mathf.Lerp(-maxRotation, maxRotation, (float)prng.NextDouble());
        float z = Mathf.Lerp(-maxRotation, maxRotation, (float)prng.NextDouble());

        // Y axis rotation can be any valid angle.
        float y = (float)prng.NextDouble() * 360f;

        return Quaternion.Euler(x, y, z);
    }
}
