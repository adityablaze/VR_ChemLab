using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

public class ChemLabelUpdater : MonoBehaviour
{
    public ChemicalData chemicalData;
    public Text uiText;
    void Start()
    {
        uiText.text = chemicalData.chemicalName.ToString();
    }
    void Update()
    {

    }
}
