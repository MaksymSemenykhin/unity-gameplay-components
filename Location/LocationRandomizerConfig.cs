using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optional config asset: list of cave prefabs and their spawn chances.
/// Reuse across scenes; override spawn area per level on the component.
/// </summary>
[CreateAssetMenu(fileName = "LocationRandomizerConfig", menuName = "Gameplay/Location Randomizer Config")]
public class LocationRandomizerConfig : ScriptableObject
{
    [System.Serializable]
    public class CaveEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance = 0.5f;
    }

    [SerializeField] private List<CaveEntry> caveEntries = new List<CaveEntry>();

    public IReadOnlyList<CaveEntry> CaveEntries => caveEntries;
}
