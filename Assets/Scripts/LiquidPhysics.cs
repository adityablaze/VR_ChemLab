using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(MeshFilter))]
public class LiquidPhysics : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The main liquid mesh (the one that wobbles)")]
    public Renderer mainRenderer;
    [Tooltip("The child mesh for the precipitate (optional)")]
    public Renderer precipitateRenderer;

    [Header("Volume Settings")]
    [Tooltip("The total capacity of this container in milliliters.")]
    public float maxVolume = 1000f; 
    
    [Tooltip("The current liquid amount.")]
    public float currentLiquidVolume = 500f;
    [Tooltip("The current precipitate amount.")]
    public float currentPptVolume = 0f;

    [Header("Chemical Content")]
    public ChemicalData currentChemical; // The main liquid identity
    public ChemicalData currentPptChemical; // The precipitate identity (if any)
    public ReactionRegistry registry;

    [Header("Wobble Settings")]
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    // Internal variables
    private Mesh mesh;
    
    // Shader Property IDs
    private static readonly int FillID = Shader.PropertyToID("_Fill");
    private static readonly int LiquidColorID = Shader.PropertyToID("_LiquidColour");
    private static readonly int SceneColorAmtID = Shader.PropertyToID("_SceneColourAmount");
    private static readonly int UpVectorID = Shader.PropertyToID("_UpVector");
    private static readonly int LocalYMinID = Shader.PropertyToID("_LocalYMin");
    private static readonly int LocalYMaxID = Shader.PropertyToID("_LocalYMax");
    private static readonly int WobbleXID = Shader.PropertyToID("_WobbleX");
    private static readonly int WobbleZID = Shader.PropertyToID("_WobbleZ");

    // Wobble Physics Variables
    private Vector3 lastPos;
    private Vector3 velocity;
    private Vector3 lastRot;
    private Vector3 angularVelocity;
    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float pulse;
    private float time = 0.5f;

    void Start()
    {
        // Auto-assign main renderer if missing
        if (mainRenderer == null) mainRenderer = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().mesh;

        SendMeshBounds();
        UpdateAllVisuals();
    }

    void SendMeshBounds()
    {
        if (mesh == null) return;
        Bounds bounds = mesh.bounds;

        // Send bounds to BOTH renderers so they fill correctly
        if(mainRenderer != null)
        {
            mainRenderer.material.SetFloat(LocalYMinID, bounds.min.y);
            mainRenderer.material.SetFloat(LocalYMaxID, bounds.max.y);
        }
        if(precipitateRenderer != null)
        {
            precipitateRenderer.material.SetFloat(LocalYMinID, bounds.min.y);
            precipitateRenderer.material.SetFloat(LocalYMaxID, bounds.max.y);
        }
    }

    void Update()
    {
        currentLiquidVolume = Mathf.Clamp(currentLiquidVolume, 0, maxVolume);
        currentPptVolume = Mathf.Clamp(currentPptVolume, 0, maxVolume - currentLiquidVolume); // Can't exceed total space

        float liquidFill = currentLiquidVolume / maxVolume;
        float pptFill = currentPptVolume / maxVolume;

        if(mainRenderer) mainRenderer.material.SetFloat(FillID, liquidFill);
        if(precipitateRenderer) precipitateRenderer.material.SetFloat(FillID, pptFill);

        Vector3 localUp = transform.InverseTransformDirection(Vector3.up);
        if(mainRenderer) mainRenderer.material.SetVector(UpVectorID, localUp);
        if(precipitateRenderer) precipitateRenderer.material.SetVector(UpVectorID, localUp);
        if (mainRenderer != null)
        {
            time += Time.deltaTime;
            // Decrease wobble
            wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * Recovery);
            wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * Recovery);

            pulse = 2 * Mathf.PI * WobbleSpeed;
            wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
            wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

            velocity = (lastPos - transform.position) / Time.deltaTime;
            angularVelocity = transform.rotation.eulerAngles - lastRot;

            wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
            wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

            lastPos = transform.position;
            lastRot = transform.rotation.eulerAngles;
            mainRenderer.material.SetFloat(WobbleXID, wobbleAmountX);
            mainRenderer.material.SetFloat(WobbleZID, wobbleAmountZ);

            // SEND ZERO WOBBLE TO PRECIPITATE (It should be stable)
            if(precipitateRenderer != null)
            {
                precipitateRenderer.material.SetFloat(WobbleXID, 0);
                precipitateRenderer.material.SetFloat(WobbleZID, 0);
            }
        }
    }

    public void AddLiquid(ChemicalData incomingChemical, float amountToAdd)
    {
        if (currentLiquidVolume + currentPptVolume + amountToAdd > maxVolume) return; // Overflow check simplified
        if (currentLiquidVolume <= 0.1f && currentPptVolume <= 0.1f)
        {
            currentChemical = incomingChemical;
            currentLiquidVolume += amountToAdd;
            UpdateAllVisuals();
            return;
        }

        if (currentChemical == incomingChemical)
        {
            currentLiquidVolume += amountToAdd;
            return;
        }

        if (registry != null)
        {
            ReactionRule rule = registry.FindReaction(currentChemical, incomingChemical);

            if (rule != null)
            {
                if (rule.resultLiquid != null)
                {
                    currentChemical = rule.resultLiquid;
                }
                if (rule.hasPrecipitate && rule.resultPrecipitate != null)
                {
                    currentPptChemical = rule.resultPrecipitate;
                    currentPptVolume += amountToAdd;
                }
                else
                {
                    currentLiquidVolume += amountToAdd;
                }

                UpdateAllVisuals();
            }
            else
            {
                currentLiquidVolume += amountToAdd;
            }
        }
    }

    public void UpdateAllVisuals()
    {
        if (currentChemical != null && mainRenderer != null)
        {
            mainRenderer.material.SetColor(LiquidColorID, currentChemical.liquidColor);
            mainRenderer.material.SetFloat(SceneColorAmtID, currentChemical.sceneColourAmount);
        }

        if (currentPptChemical != null && precipitateRenderer != null)
        {
            precipitateRenderer.material.SetColor(LiquidColorID, currentPptChemical.liquidColor);
            precipitateRenderer.material.SetFloat(SceneColorAmtID, currentPptChemical.sceneColourAmount);
        }
    }
}