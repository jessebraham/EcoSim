using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AnimalSpawner
{
    // Animals will only spawn on Grass and TallGrass type nodes.
    private static MapGraph.MapNodeType[] allowedNodeTypes =
    {
        MapGraph.MapNodeType.Grass,
        MapGraph.MapNodeType.TallGrass,
    };

    private static AnimalSettings settings;
    private static PrefabSpawner  spawner;


    public static void Spawn(AnimalSettings settings, Transform container, MapGraph mapGraph, int seed)
    {
        AnimalSpawner.settings = settings;
        AnimalSpawner.spawner  = new PrefabSpawner(container, mapGraph, seed);

        SpawnAnimals();
    }


    static void SpawnAnimals()
    {
        var grassNodes = spawner.FilterNodes(allowedNodeTypes);

        foreach (AnimalSpeciesSettings speciesSettings in settings.speciesSettings)
        {
            SpawnSpecies(speciesSettings, grassNodes);
        }
    }

    static void SpawnSpecies(AnimalSpeciesSettings settings, List<MapGraph.MapNode> nodes)
    {
        int initialPopulationSize = spawner.prng.Next(
            settings.minStartingPopulation,
            settings.maxStartingPopulation
        );

        var spawnPoint = SelectSpawnPoint(nodes);
        for (int i = 0; i < initialPopulationSize; i++)
        {
            // Spawn a new instance of the prefab.
            MeshRenderer obj = spawner.SpawnPrefab(spawnPoint, settings.prefab, RandomRotation());

            // Randomize the neighbours so that we're not just iterating
            // through linearly.
            var neighbours = spawnPoint.GetNeighborNodes()
                .OrderBy(_ => spawner.prng.Next())
                .ToList();

            // Attempt to select a random neighbouring node as the next spawn
            // point.
            int j;
            for (j = 0; j < neighbours.Count; j++)
            {
                if (allowedNodeTypes.Contains(neighbours[j].nodeType)
                    && !neighbours[j].occupied)
                {
                    spawnPoint = neighbours[j];
                    break;
                }
            }

            // If none of the neighbouring nodes are valid candidates, just
            // randomly select a new one instead.
            if (j >= neighbours.Count)
            {
                spawnPoint = SelectSpawnPoint(nodes);
            }
        }
    }

    static MapGraph.MapNode SelectSpawnPoint(List<MapGraph.MapNode> candidates)
    {
        int nodeIndex;
        do
        {
            nodeIndex = spawner.prng.Next(candidates.Count);
        } while (candidates[nodeIndex].occupied);

        return candidates[nodeIndex];
    }

    static Quaternion RandomRotation()
    {
        // Y axis rotation can be any valid angle.
        float y = (float)spawner.prng.NextDouble() * 360f;
        return Quaternion.Euler(0f, y, 0f);
    }
}
