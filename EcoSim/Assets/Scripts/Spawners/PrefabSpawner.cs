using UnityEngine;

public class PrefabSpawner
{
    private Transform container;


    public PrefabSpawner(Transform container)
    {
        // Clear all elements within the specified container upon instantiation.
        this.container = container;
        ClearGameObjects();
    }


    public MeshRenderer SpawnPrefab(MeshRenderer prefab, Vector3 position, Quaternion rotation)
    {
        // Pass the parameters to the other SpawnPrefab function, using the
        // default scaling (ie. 1).
        return SpawnPrefab(prefab, position, rotation, 1f);
    }

    public MeshRenderer SpawnPrefab(MeshRenderer prefab, Vector3 position, Quaternion rotation, float scaleModifier)
    {
        MeshRenderer obj     = Object.Instantiate(prefab, position, rotation);
        obj.transform.parent = container;

        obj.transform.localScale *= scaleModifier;

        return obj;
    }


    void ClearGameObjects()
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            GameObject.DestroyImmediate(child);
        }
    }
}
