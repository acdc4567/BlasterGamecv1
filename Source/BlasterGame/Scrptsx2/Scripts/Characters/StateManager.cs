using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cover;

public class StateManager : MonoBehaviour
{
    public bool aiming;
    public bool canRun;
    public bool dontRun;
    public bool walk;
    public bool shoot;
    public bool actualShooting; //removed this variable and all references in handleshooting but kept it for camera shake
    public bool reloading;
    public bool onGround;


    public bool down;
    [HideInInspector]
    public LastStand lastStand;

    public bool crouching;
    public float stance;
    
    public bool inCover;
    public int coverDirection;
    public bool canAim;
    public bool crouchCover;
    public bool aimAtSides;    

    public bool vaulting;
    bool climb;
    public BezierCurve vaultCurve;
    public BezierCurve climbCurve;
    Vector3 curvePos;
    bool initVault;

    public bool meleeWeapon;

    public float horizontal;
    public float vertical;
    public Vector3 lookPosition;
    public Vector3 lookHitPosition;
    public LayerMask layerMask;

    public CharacterAudioManager audioManager;

    [HideInInspector]
    public int weaponAnimType;

    [HideInInspector]
    public HandleShooting handleShooting;
    [HideInInspector]
    public HandleAnimations handleAnim;
    [HideInInspector]
    public WeaponManager weaponManager;
    [HideInInspector]
    public IKHandler ikHandler;
    [HideInInspector]
    public CharacterMovement cMovement;
    [HideInInspector]
    public CoverBehaviour coverBehavior;

    [HideInInspector]
    public GameObject model;

    public bool switchCharacter;
    public int targetChar = 1;

    [HideInInspector]
    public TimeManager tm;
    public float myDelta;

    public bool dummyModel;

    void Start()
    {
        tm = TimeManager.GetInstance();

        model = transform.GetChild(0).gameObject;

        audioManager = GetComponent<CharacterAudioManager>();
        handleShooting = GetComponent<HandleShooting>();
        handleAnim = GetComponent<HandleAnimations>();
        weaponManager = GetComponent<WeaponManager>();
        cMovement = GetComponent<CharacterMovement>();
        ikHandler = GetComponent<IKHandler>();
        lastStand = GetComponent<LastStand>();
        coverBehavior = GetComponent<CoverBehaviour>();

        if (audioManager)
            audioManager.Init();
        if (handleAnim)
            handleAnim.Init();
        if (ikHandler)
            ikHandler.Init();
        if (weaponManager)
            weaponManager.Init();    
        if (handleShooting)
            handleShooting.Init();
        if (cMovement)
            cMovement.Init();
        if (lastStand)
            lastStand.Init(this);
        if (coverBehavior)
            coverBehavior.Init(this);


        if (GetComponent<InputHandler>())
        {
            if (ResourceManager.GetInstance())
            {
                ResourceManager.GetInstance().SwitchCharacterModelWithIndex(this
                                        , ResourceManager.GetInstance().activeModelIndex);
            }
        }

        if (vaultCurve)
            vaultCurve.transform.parent = null;

        if (climbCurve)
            climbCurve.transform.parent = null; 
    }

    void FixedUpdate()
    {
        myDelta = tm.GetFixDelta();

        if (!dummyModel)
        {
            if (handleShooting)
                handleShooting.Tick();
            if (weaponManager)
                weaponManager.Tick();
            if (handleAnim)
                handleAnim.Tick();
            if (audioManager)
                audioManager.Tick();
            if (ikHandler)
                ikHandler.Tick();
            if (cMovement)
                cMovement.Tick();
        }

        onGround = IsOnGround();

        walk = (inCover||dontRun||crouching);

        HandleStance();
       // HandleVault();

        if (down && lastStand)
        {
            ikHandler.bypassAngleClamp = true;
            ikHandler.Tick();
            lastStand.Tick();
            handleShooting.Tick();
            audioManager.Tick();
        }

        if (coverBehavior && !down)
        {
            coverBehavior.Tick();
        }
    }

    float targetStance;
    void HandleStance()
    {
        if(!crouching)
        {

            targetStance = 1;
        }
        else
        {
            targetStance = 0;
        }

        stance = Mathf.Lerp(stance, targetStance, Time.deltaTime * 6);

        if (stance > 1)
            stance = 1;
        if (stance < 0)
            stance = 0;
    }

    public void Vault(bool climb = false)
    {
        this.climb = false;
        this.climb = climb;

        BezierCurve curve = (climb) ? climbCurve : vaultCurve;

        curve.transform.rotation = transform.rotation;
        curve.transform.position = transform.position;

        string desiredAnimation = (climb)? "Climb" : "Vault";

        handleAnim.anim.CrossFade(desiredAnimation, 0.2f);
        curve.close = false;
        percentage = 0;
        vaulting = true;       
    }

    float percentage;
    bool ignoreVault;

    void HandleVault()
    {
        if(vaulting)
        {
            BezierCurve curve = (climb)? climbCurve : vaultCurve;

            float lineLength = curve.length;

            float speedModifier = handleAnim.anim.GetFloat("CurveSpeed");

            float speed = (climb) ? 4 * speedModifier : 6;

            float movement = speed * Time.deltaTime;

            float lerpMovement = movement / lineLength ;

            percentage += lerpMovement;

            if (percentage > 1)
            {
                vaulting = false;
            }

            Vector3 targetPosition = curve.GetPointAt(percentage);

            transform.position = targetPosition;  
        }
    }

    bool IsOnGround()
    {
        bool retVal = false;

        Vector3 origin = transform.position + new Vector3(0, 0.05f, 0);
        RaycastHit hit;

        if (Physics.Raycast(origin, -Vector3.up, out hit, 0.5f, layerMask))
        {
            retVal = true;
        }

        return retVal;
    }

    public void ChangedModelCallBack()
    {
       if(GetComponent<DualWield.Akimbo>())
        {
            GetComponent<DualWield.Akimbo>().ReInit();
        }
    }
}
