using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

    public float horizontal;
    public float vertical;
    public float mouse1;
    public float mouse2;
    public float fire3;
    public float middleMouse;
    public float mouseX;
    public float mouseY;

    [HideInInspector]
    public FreeCameraLook camProperties;
    [HideInInspector]
    public Transform camHolder;
    [HideInInspector]
    public Transform camPivot;
    [HideInInspector]
    public Transform camTrans;

    //CrosshairManager crosshairManager;
    ShakeCamera shakeCam;
    StateManager states;

    LayerMask layerMask;

    public float shakeRecoil = 0.5f;
    public float shakeMovement = 0.3f;
    public float shakeMin = 0.1f;
    float targetShake;
    float curShake;

    public bool leftPivot;
    public bool changePivot;
    public bool crouch;

    public bool fpsMode;
    bool canSwitch;
    ControllerSwitcher conSwitcher;

    CameraScripts.CameraStates cameraStates;

    void Start()
    {
        cameraStates = CameraScripts.CameraStates.GetInstance();
        camProperties = FreeCameraLook.GetInstance();
        camHolder = camProperties.transform;
        camPivot = camProperties.transform.GetChild(0);
        camTrans = camPivot.GetChild(0);
        shakeCam = camPivot.GetComponentInChildren<ShakeCamera>();

        states = GetComponent<StateManager>();

        layerMask = ~(1 << gameObject.layer | 1 << 2);
        states.layerMask = layerMask;

        conSwitcher = ControllerSwitcher.GetInstance();
       
        if(conSwitcher != null)
        {
            canSwitch = true;
        }
    }

	void FixedUpdate () {

        HandleInput();
        UpdateStates();
        HandleShake();
        
        // Find where the camera is looking
        Ray ray = new Ray(camTrans.position, camTrans.forward);
        states.lookPosition = ray.GetPoint(20);
        RaycastHit hit;

        Debug.DrawRay(ray.origin,ray.direction);

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100,layerMask))
        {
            states.lookHitPosition = hit.point;
        }
        else
        {
            states.lookHitPosition = states.lookPosition;
        }

        cameraStates.aiming = states.aiming;
        cameraStates.leftPivot = leftPivot;
        cameraStates.crouch = states.crouching;

        C_StateType targetState = C_StateType.normal;

        if (states.inCover)
        {
            camProperties.inCover = states.aiming;
            camProperties.coverDirection = states.coverDirection;

            if (states.aiming)
            {
                if (states.crouchCover && !states.aimAtSides)
                {
                    targetState = C_StateType.coverCenter;
                }
                else
                {
                    targetState = (states.coverDirection < 0) ? C_StateType.coverLeft : C_StateType.coverRight;
                }
            }
        }

        //TODO remove debug input
        if (Input.GetKeyUp(KeyCode.V))
            states.down = true;

        if (states.down)
        {
            targetState = C_StateType.down;
        }

        cameraStates.AssignCurState(targetState);
    }

    void HandleInput()
    {
        horizontal = Input.GetAxis("Horizontal");       
        vertical = Input.GetAxis("Vertical");
        mouse1 = Input.GetAxis("Fire1");
        mouse2 = Input.GetAxis("Fire2");

        middleMouse = Input.GetAxis("Mouse ScrollWheel");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        fire3 = Input.GetAxis("Fire3");
        crouch = Input.GetKeyDown(KeyCode.C);

        if (!states.aiming)
        {
            states.dontRun = Input.GetKey(KeyCode.LeftShift);
        }

        //From the Tps to Fps video
        if (canSwitch)
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                Ray ray = new Ray(camTrans.position, camTrans.forward);
                Vector3 lookPos = ray.GetPoint(20);

                if (!fpsMode)
                    conSwitcher.SwitchToFps(lookPos);
                else
                    conSwitcher.SwitchToTPS(lookPos);
            }
        }

        if (states.inCover)//If we are in cover we want to override controls
        {
            changePivot = false;
            leftPivot = (states.coverDirection < 0) ? true : false;

            if (states.aiming)//if we are aiming, we cannot move
            {
                states.horizontal = 0;
            }
        }
        else
        {   //If we are not in cover, we handle the aiming as before
            states.crouchCover = false; //and we are no longer in a crouch cover
            states.canAim = false;

            if(Input.GetKeyDown(KeyCode.E))
            {
                changePivot = !changePivot;
            }

            if (states.down)
                changePivot = false;

            leftPivot = changePivot;
        }
    }

    void UpdateStates()
    {
        //we have our inputs, so let's take care of our states
        states.canRun = !states.aiming;

        if (!states.inCover)//if we are not in cover, then walking is controller by the input
        {
            if (!states.dontRun)
                states.walk = (fire3 > 0);
            else
                states.walk = true;
        }

        states.horizontal = horizontal;
        states.vertical = vertical;

        //if we are in cover
        if(states.inCover)
        {
            //we split our input control
            //when we are in a crouch cover then we can aim anywhere
            if (states.crouchCover)
            {
                if (mouse2 > 0)
                {
                    states.aiming = true;
                }
                else
                {
                    states.aiming = false;
                }
            }
            else //else do what we did before
            {
                if (mouse2 > 0 && states.canAim)
                {
                    states.aiming = true;
                }
                else
                {
                    states.aiming = false;
                }
            }
        }
        else
        {
           
            if (!states.meleeWeapon)
                states.aiming = states.onGround && (mouse2 > 0);
            else
                states.aiming = false;
        }

        //Same us before
        if (states.aiming)
        {

            if (mouse1 > 0.5f && !states.reloading) //we want to shoot
            {
                states.shoot = true;
            }
            else
            {
                states.shoot = false;
            }
        }
        else
        {   
            states.shoot = false;    
        }

        if(crouch)
        {
            states.crouching = !states.crouching;
        }

        //if we are in half cover we always want to crouch
        if(states.crouchCover)
        {
            states.crouching = true;
        }
    }

    void HandleShake()
    {
        if(states.actualShooting && states.weaponManager.ReturnCurrentWeapon().weaponStats.curBullets > 0)
        {
            targetShake = shakeRecoil;
            camProperties.WiggleCrosshairAndCamera(0.2f);
            states.actualShooting = false;
        }
        else
        {
            if(states.vertical != 0)
            {
                targetShake = shakeMovement;
            }
            else
            {
                if(states.horizontal != 0)
                {
                    targetShake = shakeMovement;
                }
                else
                {
                    targetShake = shakeMin;
                }
            }     
        }

        curShake = Mathf.Lerp(curShake, targetShake, states.myDelta * 10);
        shakeCam.positionShakeSpeed = curShake;
    }

    void CameraCollision(LayerMask layerMask)
    {
        //Update with this later
        /*
        //Do a raycast from the pivot of the camera to the camera
        Vector3 origin = camPivot.TransformPoint(Vector3.zero);
        Vector3 direction = camPivot.TransformPoint(Vector3.zero) - camPivot.TransformPoint(Vector3.zero);
        RaycastHit hit;

     

       if (Physics.Raycast(origin, direction, out hit, Mathf.Abs(targetZ), layerMask))
        {
            //if we hit something, then find that distance
            float dis = Vector3.Distance(camPivot.position, hit.point);
          
        }
        else
        {
            if (Physics.Raycast(origin, camTrans.right, out hit, Mathf.Abs(targetZ), layerMask))
            {
                //if we hit something, then find that distance
                float dis = Vector3.Distance(camPivot.position, hit.point);
                actualZ = -dis;//and the opposite of that is where we want to place our camera
            }
            else
            {
                if (Physics.Raycast(origin, -camTrans.right, out hit, Mathf.Abs(targetZ), layerMask))
                {
                    //if we hit something, then find that distance
                    float dis = Vector3.Distance(camPivot.position, hit.point);
                    actualZ = -dis;//and the opposite of that is where we want to place our camera
                }
            }
        }
        */
    }
}
