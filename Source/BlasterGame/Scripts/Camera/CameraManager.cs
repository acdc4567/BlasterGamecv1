using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TPC.CameraScripts
{
    public class CameraManager : MonoBehaviour
    {
        public bool holdCamera;
        public bool addDefaultAsNormal;
        public bool clampAngle;
        public float clampValue = 35;
        float maxAngle;
        float minAngle;

        public StateManager states;
        public Transform target;

        #region Variables
        public float default_TurnSpeed = 1.5f;
        public float default_TurnSpeedController = 5.5f;
        public string activeStateID;
        [SerializeField]
        float moveSpeed = 5;
        [SerializeField]
        float turnSmoothing = .1f;
        [SerializeField]
        bool isController;
        public bool lockCursor;
        #endregion

        #region References
        [HideInInspector]
        public Transform pivot;
        [HideInInspector]
        public Transform camTrans;
        [HideInInspector]
        public Transform camActual;
        #endregion

        static public CameraManager singleton;

        Vector3 targetPosition;
        [HideInInspector]
        public Vector3 targetPositionOffset;
        public bool leftPivot;

        #region Internal Variables
        float x;
        float y;
        float lookAngle;
        float tiltAngle;
        float offsetX;
        float offsetY;
        float smoothX = 0;
        float smoothY = 0;
        float smoothXvelocity = 0;
        float smoothYvelocity = 0;
        #endregion

        [SerializeField]
        List<CameraState> cameraState = new List<CameraState>();
        CameraState activeState;
        CameraState defaultState;
        float turnSpeed;
        float turnSpeedController;
        LayerMask ignoreLayers;

        void Awake()
        {
            singleton = this;
        }

        void Start()
        {
            if(Camera.main.transform == null)
            { Debug.Log("You haven't assigned a camera with the tag 'MainCamera' !"); }

            camActual = Camera.main.transform;
            camTrans = Camera.main.transform.parent;
            pivot = camTrans.parent;

            //Create Default State
            CameraState cs = new CameraState();
            cs.id = "default";
            cs.minAngle = 35;
            cs.maxAngle = 35;
            cs.cameraFOV = Camera.main.fieldOfView;
            cs.cameraZ = camTrans.localPosition.z;
            cs.pivotPosition = pivot.localPosition;
            defaultState = cs;

            if(addDefaultAsNormal)
            {
                cameraState.Add(defaultState);
                defaultState.id = "normal";
            }

            activeState = defaultState;
            activeStateID = activeState.id;
            FixPositions();

            if(lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            //ignore raycast, controller, ragdoll
            ignoreLayers = ~(1 << 2 | 1 << 8 | 1 << 9);
            Camera.main.nearClipPlane = 0.01f;
        }

        void FixedUpdate()
        {
            if (target)
            {
                targetPosition = target.position + targetPositionOffset;
            }

            CameraFollow();

            if (!holdCamera)
                HandleRotation();

            FixPositions();
            
        }

        void CameraFollow()
        {
            Vector3 camPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            transform.position = camPosition;
        }

        void HandleRotation()
        {
            HandleOffsets();
            x = Input.GetAxis("Mouse X") + offsetX;
            y = Input.GetAxis("Mouse Y") + offsetY;

            HandleCrosshair();

            float targetTurnSpeed = turnSpeed;

            if (isController)
            {
                targetTurnSpeed = turnSpeedController;
            }

            if (turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnSmoothing);
            }
            else
            {
                smoothX = x;
                smoothY = y;
            }
            
            lookAngle += smoothX * targetTurnSpeed;
            
            transform.rotation = Quaternion.Euler(0, lookAngle, 0);

            tiltAngle -= smoothY * targetTurnSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, -activeState.minAngle, activeState.maxAngle);

            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
           
        }

        CameraState GetState(string id)
        {
            CameraState r = null;
            for (int i = 0; i < cameraState.Count; i++)
            {
                if (cameraState[i].id == id)
                {
                    r = cameraState[i];
                    break;
                }
            }

            return r;
        }

        public void ChangeState(string id)
        {
            if (activeState.id != id)
            {
                CameraState targetState = GetState(id);
                if(targetState == null)
                { Debug.Log("Camera state ' " + id + " ' not found! Using previous"); return; }

                activeState = targetState;
                activeStateID = activeState.id;
            }
        }

        bool changePivot;
        void FixPositions()
        {
            if (Input.GetAxis(Statics.pivotInput) < -0.5f || Input.GetButton(Statics.pivotInput))
            {
                if (!changePivot)
                {
                    leftPivot = !leftPivot;
                    changePivot = true;
                }
            }
            else
            {
                changePivot = false;
            }

            float targetZ = (activeState.useDefaultCameraZ) ? defaultState.cameraZ : activeState.cameraZ;
            float actualZ = targetZ;

            Vector3 targetPivotPosition = (activeState.useDefaultPosition) ? defaultState.pivotPosition : activeState.pivotPosition;
            float px = targetPivotPosition.x;
            targetPivotPosition.x = (leftPivot) ? px * -1 : px * 1;

            if (states != null)
            {
                if (!states.aiming)
                {
                        if (!states.crouching && !states.inCover)
                            targetPivotPosition.x += 0.8f * states.horizontal;
                        else
                            targetPivotPosition.x += 0.5f * states.horizontal;
                }

                if (!states.aiming)
                {
                    float py = targetPivotPosition.y;
                    targetPivotPosition.y = (states.crouching) ? py /1.3f : py;
                }
            }

            Vector3 pivotLocation = transform.TransformPoint(targetPivotPosition);
            Vector3 tranCenter = transform.position;
            tranCenter.y = pivotLocation.y;
            Vector3 pivotDir = pivotLocation - tranCenter;
            float pivDis = Vector3.Distance(pivotLocation, tranCenter);
            RaycastHit pivHit;
            //Debug.DrawRay(tranCenter, pivotDir * (pivDis + 0.5f));

            if (Physics.Raycast(tranCenter, pivotDir, out pivHit, pivDis + 0.5f, ignoreLayers))
            {
                leftPivot = !leftPivot;
            }

            pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetPivotPosition, Time.deltaTime * 5);

            CameraCollision(targetZ, ref actualZ);

            Vector3 targetP = camTrans.localPosition;
            targetP.z = Mathf.Lerp(targetP.z, actualZ, Time.deltaTime * 5);
            camTrans.localPosition = targetP;

            float targetFov = (activeState.useDefaultFOV)? defaultState.cameraFOV:activeState.cameraFOV;
            if (targetFov < 1)
            {
                targetFov = 2;
            }
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFov, Time.deltaTime * 5);
        }

        void CameraCollision(float targetZ, ref float actualZ)
        {
            float step = Mathf.Abs(targetZ);
            int stepCount = 2;
            float stepIncremental = step / stepCount;

            RaycastHit hit;
            Vector3 origin = pivot.position;
            Vector3 direction = -pivot.forward;
           // Debug.DrawRay(origin, direction * step, Color.blue);

            if (Physics.Raycast(origin, direction, out hit, step, ignoreLayers))
            {
                float distance = Vector3.Distance(hit.point, origin);
                actualZ = -(distance / 2);
            }
            else
            {
                for (int s = 0; s < stepCount + 1; s++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 dir = Vector3.zero;
                        Vector3 secondOrigin = origin + (direction * s) * stepIncremental;
                        //Vector3 secondOrigin = origin + direction * step;

                        switch (i)
                        {
                            case 0:
                                dir = camTrans.right;
                                break;
                            case 1:
                                dir = -camTrans.right;
                                break;
                            case 2:
                                dir = camTrans.up;
                                break;
                            case 3:
                                dir = -camTrans.up;
                                break;
                            default:
                                break;
                        }

                        //Debug.DrawRay(secondOrigin, dir * 0.5f, Color.red);
                        if (Physics.Raycast(secondOrigin, dir, out hit, 0.5f, ignoreLayers))
                        {
                            float distance = Vector3.Distance(secondOrigin, origin);
                            actualZ = -(distance / 2);
                            if (actualZ < 0.2f)
                                actualZ = 0;

                            return;
                        }
                    }
                }
            }
        }

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

        public float crosshairOffsetWiggle = 0.2f;
        void HandleCrosshair()
        {
            if (Mathf.Abs(x) > crosshairOffsetWiggle || Mathf.Abs(y) > crosshairOffsetWiggle)
            {
                WiggleCrosshairAndCamera();
            }
        }

        public void WiggleCrosshairAndCamera()
        {
            if (CrosshairManager.singleton.activeCrosshair == null)
                return;

            CrosshairManager.singleton.activeCrosshair.WiggleCrosshair();
        }

        IEnumerator LerpCameraFOV(float z)
        {
            float cur = Camera.main.fieldOfView;
            float targetFov = z;
            if(targetFov <  1)
            {
                targetFov = 2;
            }
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime * 5;
                Camera.main.fieldOfView = Mathf.Lerp(cur, targetFov, t);
                yield return null;
            }
        }

        #region Public Functions Setters
        public void SetOffsets(float x, float y)
        {
            offsetX = x;
            offsetY = y;
        }

        public void SetSpeed(float ts, float tsc)
        {
            turnSpeed = ts;
            turnSpeedController = tsc;
        }

        public void SetDefault()
        {
            turnSpeed = default_TurnSpeed;
            turnSpeedController = default_TurnSpeedController;
        }

        public void SetCameraValuesToFps()
        {           
            CrosshairManager.singleton.activeCrosshair.gameObject.SetActive(false);
        }

        public void SetCameraValuesToTps()
        {
            CrosshairManager.singleton.activeCrosshair.gameObject.SetActive(true);
        }
        #endregion
    }

    [System.Serializable]
    public class CameraState
    {
        [Header("Name of state")]
        public string id;
        [Header("Limits")]
        public float minAngle;
        public float maxAngle;
        [Header("Pivot Position")]
        public bool useDefaultPosition;
        public Vector3 pivotPosition;
        [Header("Camera Position")]
        public bool useDefaultCameraZ;
        public float cameraZ;
        [Header("Camera FOV")]
        public bool useDefaultFOV;
        public float cameraFOV;
    }
}
