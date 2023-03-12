using UnityEngine;
using System.Collections;

public class DisableGameObject : MonoBehaviour {

    public GameObject targetObject;
	
	void OnEnable()
    {
        targetObject.SetActive(true);
    }

    void OnDisable()
    {
        targetObject.SetActive(false);
    }
}
