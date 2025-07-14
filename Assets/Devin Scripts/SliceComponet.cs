using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SliceComponet : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityestimator;
    public LayerMask sliceablelayer;
    public GameObject target;
    public Material CrossSectionMaterial;
    public float cutForce;
    public Transform player;
    public ParticleSystem ketchup;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceablelayer);
        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Color colorSlice = new Color(191, 105, 38);
            Debug.DrawLine(player.position, target.transform.position, colorSlice, 2000f);


            if (target.GetComponent<SkinnedMeshRenderer>() != null)
            {
                dismemberment(target);

            }
            else
            {
                Slice(target);
            }
        }


    }
    public void Slice(GameObject target)
    {


        Vector3 velocity = velocityestimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
        Debug.Log(hull);


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
        //Debug.Log(bp);
        //Debug.Log(bp.transform);
        //Debug.Log(bp.transform.parent);
        //bp.transform.parent = null;
        //Debug.Log("LIMBS SHOULD BE FLYING");
        //setupSlicedComponent(bp);



        GameObject bp2 = new GameObject(bp.name + "BUT BETTER");
        bp2.AddComponent<MeshFilter>();
        MeshFilter bp2mesh = bp2.GetComponent<MeshFilter>();
        bp2.AddComponent<MeshRenderer>();
        MeshRenderer mesren = bp2.GetComponent<MeshRenderer>();

        //bp2.AddComponent<BoxCollider>();
        BoxCollider boco = bp2.GetComponent<BoxCollider>();
        mesren.material = bp.GetComponent<SkinnedMeshRenderer>().material;

        bp2mesh.mesh = bp.GetComponent<SkinnedMeshRenderer>().sharedMesh;



        Vector3 bpVec = bp.transform.position;
        Quaternion bpRot = bp.transform.localRotation;
        bp2.transform.position = bp.transform.position;
        bp2.transform.rotation = bp.transform.rotation;
        bp2.AddComponent<Rigidbody>();
        bp2.GetComponent<Rigidbody>().AddForce(new Vector3(5, 5, 5), ForceMode.Impulse);

        bp2.AddComponent<BoxCollider>();
        bp2.AddComponent<XRGrabInteractable>();
        //bp2.tag = "";



        GameObject kinder = new GameObject();
        kinder.transform.parent = bp2.transform;
        kinder.transform.localPosition = new Vector3(0, 0, 0);

        kinder.transform.localPosition = new Vector3(0, -0.3f, 0);

        XRGrabInteractable grabbygrab = bp2.GetComponent<XRGrabInteractable>();
        grabbygrab.attachTransform = kinder.transform;

        //ParticleSystem ketwo = Instantiate(ketchup);
        //ketwo.transform.parent = kinder.transform;
        //ketwo.transform.localPosition = new Vector3(0, 0, 0);
        //ketwo.Play();

        Destroy(bp);

    }

}
