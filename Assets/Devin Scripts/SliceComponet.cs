using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.XR.CoreUtils;

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
    public ParticleSystem ketchup;

    public float dismembermentThreshold; 

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
            // Optional: Check if sword is moving fast enough to slice
            // Vector3 velocity = velocityestimator.GetVelocityEstimate();
            // float speed = velocity.magnitude;

            // if (speed > dismembermentThreshold) // Adjust this threshold as needed
            // {
            Debug.Log("Sword collided with sliceable object: " + other.gameObject.name);
            Debug.Log("Target Layer: " + other.gameObject.layer);
            // Debug.Log("Sword speed: " + speed);

            // if (other.GetComponent<SkinnedMeshRenderer>() != null)
            // {
                dismemberment(other.gameObject);
                Debug.Log(other.gameObject.name + " should be dismembered");
            // }
            // else
            // {
            //     Slice(other.gameObject);
            // }
            // }
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
            Destroy(bp.transform.parent.gameObject);

        }

    }

}
