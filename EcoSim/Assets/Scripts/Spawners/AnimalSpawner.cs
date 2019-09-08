using System.Linq;
using UnityEngine;

public static class AnimalSpawner
{
    // Animals will only spawn on Grass and TallGrass type nodes.
    private static MapGraph.NodeType[] allowedNodeTypes =
    {
        MapGraph.NodeType.Grass,
        MapGraph.NodeType.TallGrass,
    };

    private static MapGraph      mapGraph;
    private static System.Random prng;


    public static void Spawn(MapGraph mapGraph, Transform container, AnimalSettings settings, int seed)
    {
        AnimalSpawner.mapGraph = mapGraph;
        AnimalSpawner.prng     = new System.Random(seed);
        
        var spawner = new PrefabSpawner(container);
        settings.speciesSettings.ForEach(s => SpawnSpecies(spawner, s));
    }


    static void SpawnSpecies(PrefabSpawner spawner, AnimalSpeciesSettings settings)
    {
        int initialPopulationSize = prng.Next(
            settings.minStartingPopulation,
            settings.maxStartingPopulation
        );

        // Select an initial spawn point prior to starting the loop
        MapGraph.Node spawnPoint = SelectSpawnPoint();

        for (int i = 0; i < initialPopulationSize; i++)
        {
            // Spawn a new instance of the prefab.
            MeshRenderer obj = spawner.SpawnPrefab(spawnPoint.centerPoint - mapGraph.center, settings.prefab, RandomRotation());

            // Mark the node as occupied.
            spawnPoint.occupiedByAnimal = true;

            // Randomize the neighbours so that we're not just iterating
            // through linearly.
            var neighbours = spawnPoint.GetNeighbourNodes()
                .OrderBy(_ => prng.Next())
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
                spawnPoint = SelectSpawnPoint();
            }
        }
    }

    static MapGraph.Node SelectSpawnPoint()
    {
        var nodes     = mapGraph.FilterNodes(allowedNodeTypes).ToList();
        int nodeIndex = prng.Next(nodes.Count);

        return nodes[nodeIndex];
    }

    static Quaternion RandomRotation()
    {
        // Y axis rotation can be any valid angle.
        float y = (float)prng.NextDouble() * 360f;
        return Quaternion.Euler(0f, y, 0f);
    }
}
