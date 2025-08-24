using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Transform references for player and enemy positioning
    Transform Player;
    Transform ScanPoint;

    Transform enemy;

    // Movement and detection settings
    int movementSpeed = 3;           // How fast the enemy moves toward player
    public float detectionRadius;       // How far the enemy can "see" 
    public int numRays;                 // Number of detection rays cast in a semicircle

    // Layer masks for different detection purposes
    public LayerMask raycastMask;       // Layers the enemy can detect (usually includes Player layer)
    public LayerMask notPlayerMask;     // Layers that block movement (walls, obstacles, NOT player)

    // Enemy stats and state
    // public int nivedh;                  // Enemy health points
    bool isSee;                  // Whether enemy currently sees the player
    public Animator anima;              // Animator for enemy movement animations
    public double PlayerBubble;         // Minimum distance to maintain from player (personal space)

    public float detectionAngle;      // Angle spread for detection rays

    public UnityEngine.LineRenderer scanner; // optional LineRenderer visual to toggle on/off

    public float angleRange;
    private float angleOffset;
    public float sweepSpeed = 1f; // degrees per second sweep speed (controls how fast the tilt oscillates)
    public float lineHeight; // adjust in Inspector

    [Header("Vision Mesh")]
    public Material visionMaterialDefault; // material for filled cone (use Unlit/Transparent)
    public Material visionMaterialDetected; // material for filled cone when player is detected

    private GameObject visionMeshObj;
    private Mesh visionMesh;
    private MeshFilter visionMeshFilter;
    private MeshRenderer visionMeshRenderer;
    public int meshSegments = 32; // fallback segment count when numRays is low

    // Start is called before the first frame update
    void Start()
    {
        // Find and cache the player GameObject by tag
        Player = GameObject.FindWithTag("Player").transform;
        ScanPoint = transform; // Cache this enemy's transform
        enemy = ScanPoint;


        // Create vision mesh object as a child so it follows the enemy
        visionMeshObj = new GameObject("VisionMesh");
        visionMeshObj.transform.SetParent(enemy, false);
        visionMeshObj.transform.localPosition = enemy.up * lineHeight; // initial offset; Update will correct it
        visionMeshObj.transform.localRotation = Quaternion.identity;

        visionMeshFilter = visionMeshObj.AddComponent<MeshFilter>();
        visionMeshRenderer = visionMeshObj.AddComponent<MeshRenderer>();
        visionMesh = new Mesh();
        visionMesh.name = "VisionConeMesh";
        visionMeshFilter.mesh = visionMesh;
        if (visionMaterialDefault != null)
        {
            visionMeshRenderer.material = visionMaterialDefault;
            visionMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            visionMeshRenderer.receiveShadows = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // maps sin ∈ [-1,1] → [-1,0], then scale
        angleOffset = -((Mathf.Sin(Time.time * sweepSpeed * Mathf.PI * 2f) - 1f) * 0.5f) * (angleRange * 0.5f);

        float angleIncrement = detectionAngle / (numRays - 1); // Changed to (numRays - 1) for even distribution

        // Use the enemy's local up so the cone follows enemy orientation (handles tilted enemies)
        Vector3 origin = ScanPoint.position + ScanPoint.up * lineHeight;

        // Calculate distance between enemy and player from the same origin
        double disbetwP = Vector3.Distance(origin, Player.position);

        // If player is too close, turn off the scanner visuals
        bool tooClose = disbetwP <= PlayerBubble;
        if (tooClose)
        {
            if (visionMeshRenderer != null) visionMeshRenderer.enabled = false;
            if (scanner != null) scanner.enabled = false;
        }
        else
        {
            // enable visuals when not too close (other logic may still disable later)
            if (visionMeshRenderer != null) visionMeshRenderer.enabled = true;
            if (scanner != null) scanner.enabled = true;
            // keep ScanOn true by default; specific detections can flip it
        }

        // Cast multiple rays in a fan pattern centered on enemy's forward direction
        for (int c = 0; c < numRays; c++)
        {
            // Calculate angle for this ray - centered around forward direction
            float angle = -detectionAngle / 2 + (c * angleIncrement); // Start from -half angle, go to +half angle

            Vector3 direction = Quaternion.Euler(0, angle, 0) * enemy.forward;

            // Visualize the ray in Scene view (blue lines) using the same origin
            Debug.DrawRay(origin, direction * detectionRadius, Color.blue, 0.1f);

            Vector3 endPoint = origin + direction * detectionRadius;


            // Use origin when casting rays so what the player sees matches the physics
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, detectionRadius, raycastMask))
            {
                // Check if we hit the player
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("PLAYER DETECTED");

                    Vector3 targetPosition = Player.position;

                    // Check if there are obstacles blocking the path to player from the same origin
                    if (Physics.Linecast(origin, targetPosition, notPlayerMask))
                    {
                        Debug.Log("PATH BLOCKED - Cannot move to player");
                        isAnima(anima, false); // Stop movement animation
                    }
                    else if (disbetwP <= PlayerBubble)
                    {
                        Debug.Log("TOO CLOSE TO PLAYER - stopping movement");
                        isAnima(anima, false); // Stop movement animation
                    }

                    // Check if we have clear line of sight AND maintain minimum distance
                    else if (Physics.Linecast(origin, targetPosition, raycastMask) && disbetwP > PlayerBubble)
                    {
                        Debug.Log("MOVING TOWARD PLAYER");
                        visionMeshRenderer.material = visionMaterialDetected;

                        isAnima(anima, true); // Start movement animation
                        isSee = true;

                        // Move toward player at specified speed
                        Vector3 newPosition = Vector3.MoveTowards(enemy.position, targetPosition, movementSpeed * Time.deltaTime);
                        enemy.position = newPosition;
                    }
                    else
                    {
                        Debug.Log("TOO CLOSE TO PLAYER - stopping movement");
                        isAnima(anima, false); // Stop movement animation
                    }

                    break; // Stop checking other rays once player is found
                }
                else
                {
                    Debug.Log("Detected non-player object - stopping movement");
                    isAnima(anima, false); // Stop movement animation
                    visionMeshRenderer.material = visionMaterialDefault;
                }
            }
            else
            {
                // No objects detected in this ray direction
                Debug.Log("No objects detected - stopping movement");
                isAnima(anima, false); // Stop movement animation
                visionMeshRenderer.material = visionMaterialDefault;
            }
        }

        // Update / rebuild the filled vision mesh (triangle fan)
      
            UpdateVisionMesh(origin);
        
    }

    // Rebuilds the filled cone mesh so the interior is visible
    void UpdateVisionMesh(Vector3 originWorld)
    {
        if (visionMesh == null) return;

        int segments = Mathf.Max(3, numRays);
        int rimCount = segments;

        // center vertex + rimCount+1 (closing) vertices
        Vector3[] vertices = new Vector3[1 + rimCount + 1];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[rimCount * 3];

        // make sure visionMeshObj local position matches origin
        visionMeshObj.transform.localPosition = enemy.InverseTransformPoint(originWorld);

        vertices[0] = Vector3.zero; // center at local origin
        uvs[0] = new Vector2(0.5f, 0.5f);

        // compute a flattened forward in world space, then pitch it down using tiltAngle
        Vector3 flatForwardWorld = Vector3.ProjectOnPlane(enemy.forward, enemy.up).normalized;
        if (flatForwardWorld.sqrMagnitude < 0.0001f) flatForwardWorld = enemy.forward.normalized;

        // pitch the forward vector down around the enemy's right axis
        Vector3 pitchedForwardWorld = Quaternion.AngleAxis(-angleOffset, enemy.right) * flatForwardWorld;

        // convert pitched forward into the visionMeshObj local space
        Vector3 pitchedForwardLocal = visionMeshObj.transform.InverseTransformDirection(pitchedForwardWorld).normalized;

        float halfAngle = detectionAngle * 0.5f;
        float angleStep = detectionAngle / rimCount;

        for (int i = 0; i <= rimCount; i++)
        {
            float a = -halfAngle + i * angleStep;
            // rotate around the local up axis to sweep the rim (visionMeshObj local up aligns with enemy.up)
            Vector3 dirLocal = Quaternion.AngleAxis(a, Vector3.up) * pitchedForwardLocal;
            Vector3 pointLocal = dirLocal.normalized * detectionRadius;
            vertices[i + 1] = pointLocal;

            // simple UV mapping
            uvs[i + 1] = new Vector2((dirLocal.x + 1f) * 0.5f, (dirLocal.z + 1f) * 0.5f);
        }

        // build triangles (fan from center)
        for (int i = 0; i < rimCount; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex + 0] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.uv = uvs;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
        visionMesh.RecalculateBounds();
    }


    // Controls enemy movement animation based on whether they're pursuing the player
    public void isAnima(Animator anim, bool tf)
    {
        if (anim != null)
        {
            anim.SetBool("IsSeeSee", tf); // Toggle movement animation parameter
        }
    }

}