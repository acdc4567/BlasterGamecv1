using UnityEngine;
using System.Collections;

public class ArrowLogic : MonoBehaviour {

    Rigidbody rb;

    public float speed = 100;
    public float range = 1;
    public StateManager owner;

	void Start () {

        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

	}

	void FixedUpdate ()
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position,transform.forward,out hit,range,owner.layerMask))
        {
            rb.isKinematic = true;
            transform.parent = hit.transform.parent;

            if (hit.transform.GetComponent<ShootingRangeTarget>())
            {
                hit.transform.GetComponent<ShootingRangeTarget>().HitTarget();
            }
            this.enabled = false;
        }
	}
}
