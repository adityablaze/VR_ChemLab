using UnityEngine;

[CreateAssetMenu(fileName = "ChemicalData", menuName = "Chemistry/ChemicalData")]
public class ChemicalData : ScriptableObject
{
    [Header("Identity")]
    public string chemicalName = "Water";
    
    [Header("Visuals")]
    [ColorUsage(true, true)] // First 'true' = show alpha, Second 'true' = enable HDR
    public Color liquidColor; 
    
    [ColorUsage(true, true)]
    public Color liquidTopColor;

    [Range(0f, 1f)]
    public float viscosity = 0.5f;
    [Header("Properties")]
    public bool isDangerous = false; // For spilling logic later
    // You can add more later: pH, flammability, etc.
}