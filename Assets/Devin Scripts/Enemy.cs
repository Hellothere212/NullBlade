using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Transform references for player and enemy positioning
    Transform Player;
    Transform enemy;

    // Movement and detection settings
    public int movementSpeed;           // How fast the enemy moves toward player
    public float detectionRadius;       // How far the enemy can "see" 
    public int numRays;                 // Number of detection rays cast in a semicircle
    
    // Layer masks for different detection purposes
    public LayerMask raycastMask;       // Layers the enemy can detect (usually includes Player layer)
    public LayerMask notPlayerMask;     // Layers that block movement (walls, obstacles, NOT player)
    
    // Enemy stats and state
    public int nivedh;                  // Enemy health points
    public bool isSee;                  // Whether enemy currently sees the player
    public Animator anima;              // Animator for enemy movement animations
    public double PlayerBubble;         // Minimum distance to maintain from player (personal space)

    public float detectionAngle;      // Angle spread for detection rays


    // Start is called before the first frame update
    void Start()
    {
        // Find and cache the player GameObject by tag
        Player = GameObject.FindWithTag("Player").transform;
        enemy = transform; // Cache this enemy's transform
    }

    // Update is called once per frame
    void Update()
    {
        float angleIncrement = detectionAngle / (numRays - 1); // Changed to (numRays - 1) for even distribution

        // Calculate distance between enemy and player
        double disbetwP = Vector3.Distance(enemy.position, Player.position);

        // Cast multiple rays in a fan pattern centered on enemy's forward direction
        for (int c = 0; c < numRays; c++)
        {
            // Calculate angle for this ray - centered around forward direction
            float angle = -detectionAngle / 2 + (c * angleIncrement); // Start from -half angle, go to +half angle
            Vector3 direction = Quaternion.Euler(0, angle, 0) * enemy.forward;
            
            // Visualize the ray in Scene view (blue lines) - fixed parameters
            Debug.DrawRay(enemy.position, direction * detectionRadius, Color.blue, 0.1f);

            Debug.Log("disbetwP: " + disbetwP); 

            RaycastHit hit;
            // Cast ray to detect objects within detection radius
            if (Physics.Raycast(enemy.position, direction, out hit, detectionRadius, raycastMask))
            {
                // Check if we hit the player
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("PLAYER DETECTED");

                    Vector3 targetPosition = Player.position;

                    // Check if there are obstacles blocking the path to player
                    if (Physics.Linecast(enemy.position, targetPosition, notPlayerMask))
                    {
                        Debug.Log("PATH BLOCKED - Cannot move to player");
                        isAnima(anima, false); // Stop movement animation
                    }
                    // Check if we have clear line of sight AND maintain minimum distance
                    else if (Physics.Linecast(enemy.position, targetPosition, raycastMask) && disbetwP > PlayerBubble)
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