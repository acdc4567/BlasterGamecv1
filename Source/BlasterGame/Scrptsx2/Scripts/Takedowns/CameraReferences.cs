using UnityEngine;
using System.Collections;

public class CameraReferences : MonoBehaviour {

    public Camera normalCamera;
    public Camera xray;

    void Start()
    {
        xray.gameObject.SetActive(false);
    }

    public static CameraReferences instance;
    public static CameraReferences GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }
}
