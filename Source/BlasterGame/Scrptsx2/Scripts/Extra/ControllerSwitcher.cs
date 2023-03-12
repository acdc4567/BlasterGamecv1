using UnityEngine;
using System.Collections;

public class ControllerSwitcher : MonoBehaviour {

    public bool fps;   

    public StateManager fpsController;
    public StateManager tpsController;
    public Transform tpsCamera;
    public Transform fpsCamera;

    public static ControllerSwitcher instance;
    public static ControllerSwitcher GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (fps)
        {
            tpsController.gameObject.SetActive(false);
            tpsCamera.gameObject.SetActive(false);
            fpsController.transform.gameObject.SetActive(true);
            fpsCamera.gameObject.SetActive(true);
        }
        else
        {
            tpsController.gameObject.SetActive(true);
            tpsCamera.gameObject.SetActive(true);
            fpsController.gameObject.SetActive(false);
            fpsCamera.gameObject.SetActive(false);
        }
    }  

    void FixedUpdate()
    {
        if (fps)
        {
            tpsController.transform.position = fpsController.transform.position;
        }
        else
        {
            fpsController.transform.position = tpsController.transform.position;
        }
    }

	public void SwitchToFps(Vector3 lookPosition) 
    {
        fpsController.transform.position = tpsController.transform.position;
        fpsController.transform.rotation = tpsController.transform.rotation;
        fpsController.lookPosition = lookPosition;        
 
        fpsController.gameObject.SetActive(true);
        fpsCamera.transform.gameObject.SetActive(true);
            
        tpsController.gameObject.SetActive(false);
        tpsCamera.gameObject.SetActive(false);

        fps = true;
       
	}

    public void SwitchToTPS(Vector3 lookPosition)
    {
        tpsController.transform.position = fpsController.transform.position;
        tpsCamera.transform.parent.position = tpsController.transform.position;

        tpsController.lookPosition = lookPosition;
        tpsController.transform.rotation = fpsController.transform.rotation;
        tpsCamera.transform.rotation = tpsController.transform.rotation;

        tpsController.gameObject.SetActive(true);
        tpsCamera.gameObject.SetActive(true);
        fpsController.gameObject.SetActive(false);
        fpsCamera.transform.gameObject.SetActive(false);

        fps = false;
    }
}
