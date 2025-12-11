using UnityEngine;

[RequireComponent(typeof(LiquidPhysics))]
public class LiquidPourer : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The tip of the flask where liquid comes out")]
    public Transform spout; 
    [Tooltip("Line Renderer for the stream visual")]
    public LineRenderer streamLine;

    [Header("Settings")]
    [Tooltip("Angle (in degrees) to start pouring. 0 is upright, 90 is sideways.")]
    public float pourThreshold = 45f;
    [Tooltip("Max speed of pouring in mL per second")]
    public float maxFlowRate = 100f;

    private LiquidPhysics sourceContainer;

    void Start()
    {
        sourceContainer = GetComponent<LiquidPhysics>();
        
        // Setup simple stream visual
        if (streamLine)
        {
            streamLine.positionCount = 2;
            streamLine.enabled = false;
        }
    }

    void Update()
    {
        // 1. Calculate Tilt Angle (0 = Upright, 180 = Upside Down)
        float tiltAngle = Vector3.Angle(Vector3.up, transform.up);

        // 2. Check if we should pour
        if (tiltAngle > pourThreshold && sourceContainer.currentLiquidVolume > 0)
        {
            Pour(tiltAngle);
        }
        else
        {
            // Stop Visuals
            if (streamLine) streamLine.enabled = false;
        }
    }

    void Pour(float currentTilt)
    {
        // Calculate flow strength (0.0 to 1.0) based on how far we tilted past threshold
        // Example: If threshold is 45 and we are at 90, strength is higher.
        float tiltDelta = Mathf.InverseLerp(pourThreshold, 180f, currentTilt);
        float currentFlowRate = maxFlowRate * tiltDelta;
        float amountToPour = currentFlowRate * Time.deltaTime;

        // Visuals: Start Stream
        if (streamLine)
        {
            streamLine.enabled = true;
            streamLine.SetPosition(0, spout.position);
            
            // Set Color of stream to match liquid
            Color c = sourceContainer.currentChemical.liquidColor;
            streamLine.startColor = c;
            streamLine.endColor = c;
        }

        // Raycast Physics
        RaycastHit hit;
        Debug.DrawRay(spout.position, Vector3.down * 2.0f, Color.green);
        // Cast a ray DOWN from the spout (Gravity direction)
        if (Physics.Raycast(spout.position, Vector3.down, out hit, 2.0f))
        {
            // Update Stream Visual to hit point
            if (streamLine) streamLine.SetPosition(1, hit.point);

            // Did we hit another container?
            LiquidPhysics target = hit.collider.GetComponentInParent<LiquidPhysics>();
            
            if (target != null)
            {
                // TRANSFER LOGIC
                // 1. Remove from Us
                ChemicalData pouredLiquid = sourceContainer.PourOut(amountToPour);
                
                // 2. Add to Them (Logic inside target handles reactions automatically!)
                if (pouredLiquid != null)
                {
                    target.AddLiquid(pouredLiquid, amountToPour);
                }
            }
            else
            {
                // We hit the floor/table (Spill logic goes here later)
                // For now, just remove liquid so it empties
                sourceContainer.PourOut(amountToPour);
            }
        }
        else
        {
            // Ray hit nothing (pouring into void)
            if (streamLine) streamLine.SetPosition(1, spout.position + Vector3.down * 0.5f);
            sourceContainer.PourOut(amountToPour);
        }
    }
}