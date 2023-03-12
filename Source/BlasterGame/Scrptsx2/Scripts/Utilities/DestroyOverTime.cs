using UnityEngine;
using System.Collections;

public class DestroyOverTime : MonoBehaviour {

    public float destroyTime = 1.3f;
	
    void Start()
    {
        Destroy(this.gameObject, destroyTime);
    }

}
