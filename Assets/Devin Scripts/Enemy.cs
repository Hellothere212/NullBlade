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

    public LineRenderer scanner;

    public Material scannerMaterial;

    public float angleOffset; // degrees to offset the entire fan, e.g. 15 degrees to the right

    public float lineHeight; // adjust in Inspector

    // Start is called before the first frame update
    void Start()
    {
        // Find and cache the player GameObject by tag
        Player = GameObject.FindWithTag("Player").transform;
        ScanPoint = transform; // Cache this enemy's transform
        enemy = ScanPoint;

        //setting up scanner
        scanner.positionCount = 0;
        scanner.material = scannerMaterial;
        // Make sure the LineRenderer uses world-space coordinates so positions line up with physics
        scanner.useWorldSpace = true;

    }

    // Update is called once per frame
    void Update()
    {
        float angleIncrement = detectionAngle / (numRays - 1); // Changed to (numRays - 1) for even distribution

        // Use the enemy's local up so the cone follows enemy orientation (handles tilted enemies)
        Vector3 origin = ScanPoint.position + ScanPoint.up * lineHeight;

        // Calculate distance between enemy and player from the same origin
        double disbetwP = Vector3.Distance(origin, Player.position);

        // Make room for arc points plus the origin duplicated at the end to close the fan
        scanner.positionCount = numRays + 2;
        scanner.SetPosition(0, origin);

        // Cast multiple rays in a fan pattern centered on enemy's forward direction
        for (int c = 0; c < numRays; c++)
        {
            // Calculate angle for this ray - centered around forward direction
            float angle = -detectionAngle / 2 + (c * angleIncrement); // Start from -half angle, go to +half angle

            Vector3 direction = Quaternion.Euler(0, angle, 0) * enemy.forward;

            // Visualize the ray in Scene view (blue lines) using the same origin
            Debug.DrawRay(origin, direction * detectionRadius, Color.blue, 0.1f);

            Vector3 endPoint = origin + direction * detectionRadius;
            scanner.SetPosition(c + 1, endPoint);

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

                        isAnima(anima, true); // Start movement animation
                        isSee = true;

                        // Move toward player at specified speed
                        Vector3 newPosition = Vector3.MoveTowards(enemy.position, targetPosition, movementSpeed * Time.deltaTime);
                        enemy.position = newPosition;
                    }
                    else
                    {
                        Debug.Log("TOO CLOSE TO PLAYER - stopping movement");
                    }

                    break; // Stop checking other rays once player is found
                }
                else
                {
                    Debug.Log("Detected non-player object - stopping movement");
                    isAnima(anima, false); // Stop movement animation
                }
            }
            else
            {
                // No objects detected in this ray direction
                isAnima(anima, false); // Stop movement animation
            }
        }
        // Duplicate the origin at the final index to close the LineRenderer fan
        Vector3 straightray = Quaternion.AngleAxis(angleOffset, Vector3.zero) * origin;
        scanner.SetPosition(numRays + 1, straightray);
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