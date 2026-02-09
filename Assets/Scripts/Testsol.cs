using UnityEngine;
using UnityEngine.InputSystem;

public class TestReaction : MonoBehaviour
{
    public LiquidPhysics targetBeaker;
    public ChemicalData chemicalToPour;
    public float amount = 50f;
    public InputActionReference testbutton;

    private void OnEnable()
    {
        if (testbutton != null && testbutton.action != null)
            testbutton.action.Enable();
    }

    void Update()
    {
        // Use WasPressedThisFrame to trigger only ONCE per click
        if (testbutton.action.WasPressedThisFrame())
        {
            targetBeaker.AddLiquid(chemicalToPour, amount);
            Debug.Log($"Poured {amount}ml of {chemicalToPour.chemicalName}");
        }
    }
}