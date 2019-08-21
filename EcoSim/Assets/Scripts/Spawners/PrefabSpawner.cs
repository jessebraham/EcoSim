using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabSpawner
{
    private Transform container;

    public MapGraph mapGraph;
    private Vector3 mapCenter;

    public System.Random prng;


    public PrefabSpawner(Transform container, MapGraph mapGraph, int seed)
    {
        this.container = container;

        this.mapGraph  = mapGraph;
        this.mapCenter = mapGraph.GetCenter();

        this.prng = new System.Random(seed);

        ClearGameObjects(container);
    }


    public List<MapGraph.MapNode> FilterNodes(IEnumerable<MapGraph.MapNodeType> types)
    {
        // If the node type is not present in the types enumerable, or the node
        // is occupied, we won't spawn anything on it.
        return mapGraph.nodesByCenterPosition.Values
            .Where(node => types.Contains(node.nodeType)
                           && !node.occupied)
            .ToList();
    }

    public bool ShouldSpawn(float probability)
    {
        return prng.NextDouble() < probability;
    }

    public MeshRenderer SpawnPrefab(MapGraph.MapNode node, MeshRenderer prefab, Quaternion rotation)
    {
        return SpawnPrefab(node, prefab, 1f, 0f, rotation);
    }

    public MeshRenderer SpawnPrefab(MapGraph.MapNode node, MeshRenderer prefab, float scale, float scaleDeviation, Quaternion rotation)
    {
        // Instantiate a new MeshRenderer from the provided prefab, and set its
        // parent to the container Transform.
        MeshRenderer obj = Object.Instantiate(prefab, node.centerPoint - mapCenter, rotation);
        obj.transform.parent = container;

        // Scale the object by the provided scale with an added random
        // deviation, if the values have been provided (ie. not the defaults).
        if (scale != 1f && scaleDeviation != 0f)
        {
            obj.transform.localScale *= scale + Mathf.Lerp(0, scaleDeviation, (float)prng.NextDouble());
        }

        // Mark the node as occupied.
        node.occupied = true;

        // Return the newly instantiated MeshRenderer.
        return obj;
    }


    void ClearGameObjects(Transform container)
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
}
