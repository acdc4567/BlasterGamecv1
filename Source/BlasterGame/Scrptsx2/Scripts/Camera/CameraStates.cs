using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CameraScripts
{
    public class CameraStates : MonoBehaviour
    {
        public List<States> camStates = new List<States>();

        [HideInInspector]
        public States curState;
        public float crouchY = 0.4f;
        float normalFOV = 60;
        float aimingFOV = 20;
        float curFov;

        Camera cam;
        public Transform camPivot;
        FreeCameraLook camProperties;
        TimeManager tm;

        [HideInInspector]
        public bool aiming;
        [HideInInspector]
        public bool crouch;
        [HideInInspector]
        public bool leftPivot;


        void Start()
        {
            tm = TimeManager.GetInstance();
            camProperties = FreeCameraLook.GetInstance();

            for (int i = 0; i < camStates.Count; i++) //init the pivot position
            {
                States s = camStates[i];
                s.pivotPosition += camPivot.localPosition;
            }

            AssignCurState(C_StateType.normal);
            camProperties.overrideTarget = true;
        }

        void Update()
        {
            Vector3 targetPivotPosition = curState.pivotPosition;
            Vector3 rootPosition = camProperties.target.position;
            Vector3 targetPosition = curState.targetPosition + rootPosition;

            if (!curState.ignoreY)
            {
                if (crouch)
                {
                    targetPosition.y -= crouchY;
                }
            }

            camProperties.newTargetPosition = targetPosition;

            if(leftPivot)
            {
                targetPivotPosition.x = -targetPivotPosition.x;
            }

            camPivot.localPosition = Vector3.Lerp(camPivot.localPosition, targetPivotPosition, tm.GetDelta() * 5);

            float targetFov = normalFOV;

            if (aiming)
            {
                targetFov = aimingFOV;
            }

            camProperties.coverAngleMin = curState.minAngle;
            camProperties.coverAngleMax = curState.maxAngle;

            curFov = Mathf.Lerp(curFov, targetFov, tm.GetDelta() * 5);

            if (curFov < 1)
                curFov = 1;

            Camera.main.fieldOfView = curFov;
        }

        public void AssignCurState(C_StateType t)
        {
            States retVal = null;

            for (int i = 0; i < camStates.Count; i++)
            {
               if(camStates[i].type == t)
                {
                    retVal = camStates[i];
                    break;
                }
            }

            if (retVal == null)
            {
                Debug.Log("no camera state of " + t.ToString() + " type found!");
                return;
            }
            
            curState = retVal;
        }

        public States ReturnState(C_StateType t)
        {
            States retVal = null;

            for (int i = 0; i < camStates.Count; i++)
            {
                if (camStates[i].type == t)
                {
                    retVal = camStates[i];
                    break;
                }
            }

            if (retVal == null)
                Debug.Log("no camera state of " + t.ToString() + " type found!");

            return retVal;
        }

        [System.Serializable]
        public class States
        {
            public C_StateType type;
            public Vector3 pivotPosition;
            public Vector3 targetPosition;     
            public float minAngle;
            public float maxAngle;
            public bool ignoreY;          
        }

        public static CameraStates instance;
        public static CameraStates GetInstance()
        {
            if(instance == null)
                Debug.Log("No CameraStates Singleton found! Fix this!");

            return instance;
        }

        void Awake()
        {
            instance = this;
        }
    }
}

public enum C_StateType
{
    normal,
    coverLeft,
    coverRight,
    coverCenter,
    other,
    down
}
