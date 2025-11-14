using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(MeshFilter))]
public class LiquidPhysics : MonoBehaviour
{
    // Your wobble settings
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    // Shader Property IDs (for performance)
    private static readonly int WobbleXID = Shader.PropertyToID("_WobbleX");
    private static readonly int WobbleZID = Shader.PropertyToID("_WobbleZ");
    private static readonly int LocalUpVectorID = Shader.PropertyToID("_UpVector");
    private static readonly int LocalYMinID = Shader.PropertyToID("_LocalYMin");
    private static readonly int LocalYMaxID = Shader.PropertyToID("_LocalYMax");

    private Renderer rend;
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
        SendMeshBounds();
    }

    // This is the new, crucial part for fixing your fill range
    void SendMeshBounds()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("LiquidPhysics needs a MeshFilter with a mesh.", this);
            return;
        }

        // Get the LOCAL bounds of the mesh
        Bounds localBounds = meshFilter.mesh.bounds;
        
        // Send the min and max Y-values to the shader
        // This will let the shader normalize the fill 0-1 correctly
        rend.material.SetFloat(LocalYMinID, localBounds.min.y);
        rend.material.SetFloat(LocalYMaxID, localBounds.max.y);
    }

    private void Update()
    {
        time += Time.deltaTime;
        
        // --- This is the new gravity logic ---
        // 1. Get world "up" (0, 1, 0)
        // 2. Transform it into this object's LOCAL space
        Vector3 localUp = transform.InverseTransformDirection(Vector3.up);
        rend.material.SetVector(LocalUpVectorID, localUp);
        // --- End new logic ---


        // --- Your existing wobble logic (unchanged) ---
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * Recovery);

        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        rend.material.SetFloat(WobbleXID, wobbleAmountX);
        rend.material.SetFloat(WobbleZID, wobbleAmountZ);

        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}