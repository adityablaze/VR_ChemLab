using UnityEngine;

[RequireComponent(typeof(LiquidPhysics))]
public class LiquidPourer : MonoBehaviour
{
    [Header("Setup")]
    public Transform spout;
    public LineRenderer streamLine;

    [Header("Settings")]
    public float pourThreshold = 45f;
    public float maxFlowRate = 100f;

    [Header("Hazard Settings")]
    [Tooltip("Drag your AcidSpill prefab here")]
    public GameObject acidSpillPrefab;
    [Tooltip("Minimum time between spawning spills (prevents lag)")]
    public float spillCooldown = 1.0f;

    private LiquidPhysics sourceContainer;
    private float lastSpillTime = 0f;

    void Start()
    {
        sourceContainer = GetComponent<LiquidPhysics>();

        if (streamLine)
        {
            streamLine.positionCount = 2;
            streamLine.enabled = false;
        }
    }

    void Update()
    {
        float tiltAngle = Vector3.Angle(Vector3.up, transform.up);

        if (tiltAngle > pourThreshold && sourceContainer.currentLiquidVolume > 0)
        {
            Pour(tiltAngle);
        }
        else
        {
            if (streamLine) streamLine.enabled = false;
        }
    }

    void Pour(float currentTilt)
    {
        // Calculate Amount
        float tiltDelta = Mathf.InverseLerp(pourThreshold, 180f, currentTilt);
        float currentFlowRate = maxFlowRate * tiltDelta;
        float amountToPour = currentFlowRate * Time.deltaTime;

        // Visuals
        if (streamLine)
        {
            streamLine.enabled = true;
            streamLine.SetPosition(0, spout.position);

            if (sourceContainer.currentChemical != null)
            {
                Color c = sourceContainer.currentChemical.liquidColor;
                streamLine.startColor = c;
                streamLine.endColor = c;
            }
        }

        // Raycast Physics
        RaycastHit hit;
        if (Physics.Raycast(spout.position, Vector3.down, out hit, 2.0f))
        {
            if (streamLine) streamLine.SetPosition(1, hit.point);

            LiquidPhysics target = hit.collider.GetComponentInParent<LiquidPhysics>();

            if (target != null)
            {
                // TRANSFER LOGIC (Pouring into another flask)
                ChemicalData pouredLiquid = sourceContainer.PourOut(amountToPour);
                if (pouredLiquid != null) target.AddLiquid(pouredLiquid, amountToPour);
            }
            else
            {
                // 1. Check if the chemical causes a hazard
                // (Make sure your Chemical Data name actually contains "H2SO4")
                if (sourceContainer.currentChemical != null && sourceContainer.currentChemical.isDangerous)
                {
                    SpawnHazard(hit.point, hit.normal);
                }

                // 2. Waste the liquid
                sourceContainer.PourOut(amountToPour);
            }
        }
        else
        {
            // Poured into void
            if (streamLine) streamLine.SetPosition(1, spout.position + Vector3.down * 0.5f);
            sourceContainer.PourOut(amountToPour);
        }
    }

    void SpawnHazard(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Don't spawn if we just spawned
        if (Time.time - lastSpillTime < spillCooldown) return;
        lastSpillTime = Time.time;

        if (acidSpillPrefab != null)
        {
            Vector3 spawnPos = hitPoint + (hitNormal * 0.1f);
            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hitNormal);
            Instantiate(acidSpillPrefab, spawnPos, spawnRot);
        }
    }
}