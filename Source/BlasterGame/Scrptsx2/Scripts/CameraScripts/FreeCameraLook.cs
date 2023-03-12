using UnityEngine;

public class FreeCameraLook : Pivot {

	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float turnSpeed = 1.5f;
	[SerializeField] private float turnsmoothing = .1f;
	[SerializeField] private float tiltMax = 75f;
	[SerializeField] private float tiltMin = 45f;
	[SerializeField] private bool lockCursor = false;

	public float lookAngle;
	private float tiltAngle;

	private const float LookDistance = 100f;

	private float smoothX = 0;
	private float smoothY = 0;
	private float smoothXvelocity = 0;
	private float smoothYvelocity = 0;

    public float crosshairOffsetWiggle = 0.2f;
    CrosshairManager crosshairManager;

    //Apply offset to the target of the camera
    public bool overrideTarget;
    public Vector3 newTargetPosition;
    //Add cover limits on the lookAngle
    public float coverAngleMax;
    public float coverAngleMin;
    public bool inCover;
    public int coverDirection;  

    //add the singleton
    public static FreeCameraLook instance;
    
    public static FreeCameraLook GetInstance()
    {
        return instance;
    }

	protected override void Awake()
	{
        instance = this;

		base.Awake();

		cam = GetComponentInChildren<Camera>().transform;
		pivot = cam.parent.parent; //take the correct pivot
	}

    protected override void Start()
    {
        base.Start();

        if (lockCursor)
           Cursor.lockState = CursorLockMode.Locked;

        crosshairManager = CrosshairManager.GetInstance();

        //init the new target position so it doesn't fly the first time we need it
        if(target)
            newTargetPosition = target.position;
    }
	
	// Update is called once per frame
    protected override	void Update ()
	{
		base.Update();

		HandleRotationMovement();

	}

	protected override void Follow (float deltaTime)
	{
        //We now want to be able to override the position the camera is following
        Vector3 tp = target.position;

        //update accordingly
        if(overrideTarget) 
        {
            tp = newTargetPosition;
        }
        else
        {
            newTargetPosition = tp;
        }

        Vector3 targetPosition = Vector3.Lerp(transform.position, tp, deltaTime * moveSpeed);

        transform.position = targetPosition;

	}

	void HandleRotationMovement()
	{
        HandleOffsets();

		float x = Input.GetAxis("Mouse X") + offsetX;
		float y = Input.GetAxis("Mouse Y") + offsetY;

        if (turnsmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnsmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnsmoothing);
        }
        else
        {
            smoothX = x;
            smoothY = y;
        }
 
        if (!inCover) //if we are not in cover, same as before
        {
            lookAngle += smoothX * turnSpeed;
        }
        else //if not, then here's where the fun starts
        {
            //find the angle between where the player model is looking and the world forward
            float angleFromWorldForward = Vector3.Angle(target.forward, Vector3.forward);
            
            //use that to find our world orientation
            int dot = DotOrientation(angleFromWorldForward);

            //now take our world orientation and find the maximum and minimum angles it can look
            float maxAngle = (angleFromWorldForward * dot) + coverAngleMax;
            float minAngle = (angleFromWorldForward * dot) + coverAngleMin;

            //add to the look angle
            lookAngle += smoothX * turnSpeed;

            //but clamp it to the values we found above
            lookAngle = Mathf.Clamp(lookAngle, minAngle, maxAngle);

            //we do it this way because look angle is relative to world positin values
        }

        //reset the look angle when it does a full circle
        if (lookAngle > 360)
            lookAngle = 0;
        if (lookAngle < -360)
            lookAngle = 0;

		transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

		tiltAngle -= smoothY * turnSpeed;
		tiltAngle = Mathf.Clamp (tiltAngle, -tiltMin, tiltMax);

		pivot.localRotation = Quaternion.Euler(tiltAngle,0,0);

        if (x > crosshairOffsetWiggle || x < -crosshairOffsetWiggle || y > crosshairOffsetWiggle || y < -crosshairOffsetWiggle)
        {
            WiggleCrosshairAndCamera(0);
        }
	}

    int DotOrientation(float angleFromWorldForward)
    {
        //to find the world orientation
        
        //we need to know if the north is in front of us or behind us
        float NSdot = Vector3.Dot(target.forward, Vector3.forward);
        //we also need to know if the east is in front of us or behind us
        float WEdot = Vector3.Dot(target.forward, Vector3.right);
        //the above variables basically returns -1 to 1

        int retVal = 0;
        
        //First we will check for north
        if(NSdot > 0)
        {
            //if we are looking towards the north

            //then we need to see which is closer, the north or West/East

            // basically if it's over 45 degress it's not the north anymore
            if (angleFromWorldForward > 45) 
            {
                retVal = WestOrEast(WEdot);
            }
            else//if it is under 45, then it's the north
            {
                //Debug.Log("North " + coverDirection);                
                retVal = -coverDirection;
            }
        }
        else//same for the south
        {
            //if it's over 45 degrees from the south 
            if (angleFromWorldForward > 45)
            {
                //look to see if it's the west or east
                retVal = WestOrEast(WEdot);
            }
            else//if it's not, we are looking at the south
            {
                //Debug.Log("south " + -coverDirection);
                retVal = -coverDirection;
            }
        }

        return retVal;
    }

    int WestOrEast(float WEdot)
    {
        //So we know it's not north or south
        int retVal = 0;

        //then the dot value will determine if it's the West or East
        if (WEdot < 0)
        {
            //Depending on what cover position we are in (so what aiming animation is going to play)
            //we want to switch the multiplier to a negative or a positive value
            //basicaly we find how to switch by either praying to a conception of a higher entity
            //or we just test the crap out of it

            //Debug.Log("west " + coverDirection);      
            if (coverDirection > 0)
                retVal = -coverDirection;
            else
                retVal = coverDirection;
        }
        else
        {
            //Debug.Log("east " + coverDirection);
            if (coverDirection < 0)
                retVal = -coverDirection;
            else
                retVal = coverDirection;
        }

        return retVal;
    }

    float offsetX;
    float offsetY;

    void HandleOffsets()
    {
        if (offsetX != 0)
        {
            offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
        }

        if (offsetY != 0)
        {
            offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
        }
    }

    public void WiggleCrosshairAndCamera(float kickback)
    { 
        crosshairManager.activeCrosshair.WiggleCrosshair();

        offsetY = kickback;
    }


}
