using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.XR.CoreUtils;
using System;
using System.Collections;
using System.Collections.Generic;

public class SliceComponet : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityestimator;
    public LayerMask sliceablelayer;

    GameObject target;
    public Material CrossSectionMaterial;
    public float cutForce;
    public Transform player;


    public ParticleSystem sparks;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void FixedUpdate()
    {
    }

    // This method will be called when the sword collides with a trigger collider
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Sword collided");
        // Check if the collided object is on the sliceable layer
        if (IsOnSliceableLayer(other.gameObject))
        {
            Debug.Log("you have hit something sliceable");

        
            Debug.Log("Sword collided with sliceable object: " + other.gameObject.name);
            Debug.Log("Target Layer: " + other.gameObject.layer);
            // Debug.Log("Sword speed: " + speed);

           
            dismemberment(other.gameObject);
            Debug.Log(other.gameObject.name + " should be dismembered");
       
        }
    }

    public void dismemberment(GameObject bp)
    {

        if (bp.transform.parent == null)
        {
            Debug.Log("No parent found, cannot dismember");
            return;
        }
        else
        {
            Debug.Log("Parent found, dismembering");

            // Add 90 degrees rotation to the Y axis
            Quaternion sparksRotation = getSparksRotation(bp.transform.parent.name);
            // * Quaternion.Euler(90, 0, 0);
            
            ParticleSystem newSparks =
            Instantiate(sparks, bp.transform.parent.position, sparksRotation);

           newSparks.transform.SetParent(bp.transform.parent.transform.parent.transform.parent,true);
           
            // Enable looping
            var main = newSparks.main;
            main.loop = true;
            
            newSparks.Play();
            
            // Stop and destroy after specified duration
            Destroy(newSparks.gameObject, 10f);
            
            Destroy(bp.transform.parent.gameObject);

        }

    }


    public Quaternion getSparksRotation(String BPname)
    {
        switch (BPname)
        {
            case "Neck":
                return Quaternion.Euler(168.752f, -35.54102f, -7.932983f);
            case "Shoulder1.L":
                return Quaternion.Euler(317.674622f, 90.207756f, 89.6203537f);
            case "Shoulder1.R":
                return Quaternion.Euler(324.003723f, 198.84523f, 139.661102f);
            case "Leg0.L":
                return Quaternion.Euler(26.5641155f, 159.09317f, 170.650665f);
            case "Leg0.R":
                return Quaternion.Euler(5.8005619f, 158.534683f, 197.396652f);

            default:
                return Quaternion.Euler(0, 0, 0);
        }
    }



    private bool IsOnSliceableLayer(GameObject obj)
    {
        return (sliceablelayer.value & (1 << obj.layer)) != 0;
    }
    public void Slice(GameObject target)
    {


        Vector3 velocity = velocityestimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
        Debug.Log("You have hit an object");


        Enemy nived = target.GetComponent<Enemy>();

        if (nived != null)
        {
            nived.nivedh -= 1;
            if (nived.nivedh > 0)
            {
                return;
            }
        }

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, CrossSectionMaterial);
            setupSlicedComponent(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(target, CrossSectionMaterial);
            setupSlicedComponent(lowerHull);

            upperHull.layer = LayerMask.NameToLayer("Sliceable");
            lowerHull.layer = LayerMask.NameToLayer("Sliceable");


            Destroy(target);
        }
    }
    public void setupSlicedComponent(GameObject slicedObject)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
    }



}
