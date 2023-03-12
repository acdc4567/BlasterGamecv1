using UnityEngine;
using System.Collections;

public class PassReference : MonoBehaviour {

    TakedownReferences tr;

    void Start()
    {
        tr = GetComponentInParent<TakedownReferences>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<TakedownPlayer>())
        {
            other.GetComponent<TakedownPlayer>().enRef = tr;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<TakedownPlayer>())
        {
            if(other.GetComponent<TakedownPlayer>().enRef == tr)
            {
                other.GetComponent<TakedownPlayer>().enRef = null;
            }
        }
    }
}
