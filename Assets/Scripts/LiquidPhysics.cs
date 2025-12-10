using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(MeshFilter))]
public class LiquidPhysics : MonoBehaviour
{
    [Header("Volume Settings")]
    [Tooltip("The total capacity of this container in milliliters.")]
    public float maxVolume = 1000f; 
    
    [Tooltip("The current liquid amount in milliliters.")]
    public float currentVolume = 500f;

    [Header("Wobble Settings")]
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    // Internal variables
    private Renderer rend;
    private Mesh mesh;
    
    // Shader Property IDs (Optimization)
    private static readonly int FillID = Shader.PropertyToID("_Fill"); // Make sure this matches your Shader Graph!
    private static readonly int UpVectorID = Shader.PropertyToID("_UpVector");
    private static readonly int LocalYMinID = Shader.PropertyToID("_LocalYMin");
    private static readonly int LocalYMaxID = Shader.PropertyToID("_LocalYMax");
    private static readonly int WobbleXID = Shader.PropertyToID("_WobbleX");
    private static readonly int WobbleZID = Shader.PropertyToID("_WobbleZ");

    // Wobble Math Variables
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
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().mesh;

        SendMeshBounds();
    }

    void SendMeshBounds()
    {
        if (mesh == null) return;

        // 1. Measure the liquid mesh
        Bounds bounds = mesh.bounds;
        
        // 2. Send the Top and Bottom Y-values to the shader
        // This ensures 0 volume is at the bottom, and Max volume is at the top.
        rend.material.SetFloat(LocalYMinID, bounds.min.y);
        rend.material.SetFloat(LocalYMaxID, bounds.max.y);
    }

    void Update()
    {
        // --- 1. VOLUME LOGIC ---
        // Ensure volume stays within bounds (0 to Max)
        currentVolume = Mathf.Clamp(currentVolume, 0, maxVolume);

        // Calculate the percentage (0.0 to 1.0)
        float fillPercent = currentVolume / maxVolume;

        // Send to Shader
        rend.material.SetFloat(FillID, fillPercent);


        // --- 2. GRAVITY LOGIC ---
        // Calculate "Up" in local space so the liquid stays level
        Vector3 localUp = transform.InverseTransformDirection(Vector3.up);
        rend.material.SetVector(UpVectorID, localUp);


        // --- 3. WOBBLE LOGIC (Unchanged) ---
        time += Time.deltaTime;
        
        // Decrease wobble over time
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * Recovery);

        // Sine wave pulse
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        rend.material.SetFloat(WobbleXID, wobbleAmountX);
        rend.material.SetFloat(WobbleZID, wobbleAmountZ);

        // Velocity calculation
        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        // Add to wobble
        wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}