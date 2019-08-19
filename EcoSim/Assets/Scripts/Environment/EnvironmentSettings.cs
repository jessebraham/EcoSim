using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Environment Settings", fileName = "EnvironmentSettings")]
public class EnvironmentSettings : ScriptableObject
{
    public event System.Action OnValuesUpdated;


    public MeshRenderer coniferousTreePrefab;
    public MeshRenderer deciduousTreePrefab;
    public MeshRenderer rockPrefab;

    [Space(10)]

    [Range(0, 1)]
    public float coniferousProbability = 0.1f;
    [Range(0, 1)]
    public float deciduousProbability = 0.1f;
    [Range(0, 1)]
    public float treeScale = 1.0f;
    [Range(0, 1)]
    public float treeScaleDeviation = 0.25f;

    [Space(10)]

    [Range(0, 2)]
    public float rockProbability = 0.1f;
    [Range(0, 1)]
    public float rockScale = 1.0f;
    [Range(0, 2)]
    public float rockScaleDeviation = 0.25f;

    
#if UNITY_EDITOR
public void NotifyOfUpdatedValues() {
		UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        OnValuesUpdated?.Invoke();
    }
#endif
}
