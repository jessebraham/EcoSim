using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Animals/Animal Settings", fileName = "AnimalSettings")]
public class AnimalSettings : ScriptableObject
{
    public List<AnimalSpeciesSettings> speciesSettings;
}

[System.Serializable]
public class AnimalSpeciesSettings
{
    public MeshRenderer prefab;

    [Space(10)]

    [Range(0, 100)]
    public int minStartingPopulation;
    [Range(0, 1000)]
    public int maxStartingPopulation;

    [Space(10)]

    [Range(0, 100)]
    public int baseHealthPoints;
    [Range(0, 25)]
    public int baseMovementSpeed;

    [Space(10)]

    [Range(0, 5)]
    public float runSpeedMultiplier;
    [Range(0, 5)]
    public float swimSpeedMultiplier;
}
