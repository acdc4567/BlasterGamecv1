using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TakedownPlayer : MonoBehaviour {

    StateManager states;
    InputHandler ih;

    TakedownCinematic tdManager;
    TakedownReferences plRef;
    [HideInInspector]
    public TakedownReferences enRef;

    GameObject crossCanvas;
    WeaponReferenceBase prevWeapon;

    bool initText;
    Text UItext;

    public int takedown;
    public bool xray;

	void Start () {
        tdManager = GetComponentInChildren<TakedownCinematic>();
        UItext = CrosshairManager.GetInstance().pickItemsText;

        plRef = GetComponent<TakedownReferences>();
        plRef.Init();

        tdManager.mainCameraRig = FreeCameraLook.GetInstance().gameObject;
        tdManager.mainCamera = Camera.main.transform;

        crossCanvas = CrosshairManager.GetInstance().transform.parent.gameObject;

        states = GetComponent<StateManager>();
        ih = GetComponent<InputHandler>();
	}
	
    void FixedUpdate()
    {
        if (enRef)
        {
            if(Input.GetKeyUp(KeyCode.X))
            {
                if (!tdManager.runTakedown)
                {
                    tdManager.t_index = takedown;
                    tdManager.xray = xray;

                    ih.enabled = false;
                    states.dummyModel = true;
                    crossCanvas.SetActive(false);

                    states.cMovement.rb.velocity = Vector3.zero;

                    prevWeapon = states.weaponManager.ReturnCurrentWeapon();

                    states.weaponManager.SwitchWeaponWithTargetWeapon(
                        tdManager.takedownList[tdManager.t_index].timeline.takedownWeapon
                        );

                    plRef.Init();

                    tdManager.playerRef = plRef;
                    tdManager.enemyRef = enRef;
                    tdManager.runTakedown = true;
                }
             }

            if (!initText)
            {
                UItext.gameObject.SetActive(true);
                UItext.text = "Press X for Takedown";
                initText = true;
            }
        }
        else
        {
            if(initText)
            {
                UItext.gameObject.SetActive(false);
                initText = false;
            }
        }

    }

    public void EndTakedown()
    {
        Debug.Log("ending");

        tdManager.CloseTakedown();

        ih.enabled = true;
        states.dummyModel = false;
        states.weaponManager.SwitchWeaponWithTargetWeapon(
                      prevWeapon);

        tdManager.runTakedown = false;
        tdManager.enemyRef = null;

        crossCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;        

    }
}
