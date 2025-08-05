using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Defines the player, enemy, and target (if Enemy sees player is target)
    public Transform Player;
    public Transform enemy;
    public Transform Target;

    public int movementSpeed;
    public float detectionRadius; // The radius of the detection zone
    public int numRays; // Number of rays to cast in the circular pattern
    public LayerMask raycastMask;
    public LayerMask notPlayerMask;
    public int nivedh; //Health for enemy
    public bool isSee;
    public Animator anima;
    public double PlayerBubble;


    

    // Start is called before the first frame update
    // Idea for later, If enemy has gotten hit a couple times, layer = sliceable
    void Start()
    {

        Player = GameObject.FindWithTag("Player").transform;
        enemy = transform;


    }

    // Update is called once per frame
    void Update()
    {

        // Calculate angle between rays
        float angleIncrement = 360f / numRays;
        double disbetwP = Vector3.Distance(enemy.position, Player.position);

        // Cast rays in a circular pattern
        for (int c = 0; c < numRays; c++)
        {
            float angle = c * angleIncrement;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * enemy.forward;
            Debug.DrawRay(enemy.position, direction, Color.blue, detectionRadius);

            RaycastHit hit;
            if (Physics.Raycast(enemy.position, direction, out hit, detectionRadius, raycastMask))
            {

                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("PLAYER PLAYER");

                    Vector3 targetPosition = Player.position;
                    if (Physics.Linecast(enemy.position, (targetPosition), notPlayerMask))
                    {
                        Debug.Log("I DONT LIKE TO MOVE IT MOVE IT");
                        isAnima(anima, false);

                    }
                    else if (Physics.Linecast(enemy.position, targetPosition, raycastMask) && disbetwP > PlayerBubble)
                    {
                        Debug.Log("I LIKE TO MOVE IT MOVE IT");

                        isAnima(anima, true);
                        isSee = true;
                        Vector3 newPosition = Vector3.MoveTowards(enemy.position, targetPosition, movementSpeed * Time.deltaTime);
                        enemy.position = newPosition;

                    }


                    break; // Stop checking other rays

                }



                else
                {
                    Debug.Log("I want to stop moving moving it");
                    isAnima(anima, false);

                }
            }
            else
            {
                isAnima(anima, false);

            }
        }

    }


    public void isAnima(Animator anim, bool tf)
    {
        if (anim != null)
        {
            anim.SetBool("IsSeeSee", tf);

        }
    }
}