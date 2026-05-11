using UnityEngine;

[CreateAssetMenu(fileName = "SimulationPreset", menuName = "Scriptable Objects/SimulationPreset")]
public class SimPreset : ScriptableObject
{
    public SimpPrefab[] presets;
    
    public GameObject find(string inpt)
    {
        foreach (var a in presets)
        {
            if (a.name == inpt)
            {
                return a.prefab;
            }
        }
        return null;
    }
}

[System.Serializable]
public struct SimpPrefab
{
    public string name;
    public GameObject prefab;
}
