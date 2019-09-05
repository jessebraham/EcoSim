using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner
{
    private Transform container;


    public PrefabSpawner(Transform container)
    {
        this.container = container;
        ClearGameObjects();
    }


    public MeshRenderer SpawnPrefab(Vector3 location, MeshRenderer prefab, Quaternion rotation)
    {
        return SpawnPrefab(location, prefab, 1f, rotation);
    }

    public MeshRenderer SpawnPrefab(Vector3 location, MeshRenderer prefab, float scaleModifier, Quaternion rotation)
    {
        MeshRenderer obj     = Object.Instantiate(prefab, location, rotation);
        obj.transform.parent = container;

        obj.transform.localScale *= scaleModifier;

        return obj;
    }


    void ClearGameObjects()
    {
        var children = new List<GameObject>();
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
