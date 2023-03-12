using UnityEngine;
using System.Collections;
//using UnityStandardAssets.CinematicEffects;

public class ControlDOF : MonoBehaviour {

    public StateManager states;
    //public DepthOfField dof;

    public DOFvalues defValues;
    public DOFvalues aimingValues;
    public DOFvalues cinematic;
    public DOFvalues closeup;

    void Start()
    {
      /*  defValues.focusPlane = dof.focusPlane;
        defValues.focusRange = dof.focusRange;
        defValues.fstops = dof.fStops;
        defValues.targetTrans = dof.focusTransform;  */
    }

    void Update()
    { 
        if(states == null)
            states = FreeCameraLook.GetInstance().target.GetComponent<StateManager>();

        if(!states.dummyModel)    
        {
            if(states.aiming)
            {
                ChangeDOFValues(1);
            }
            else
            {
                ChangeDOFValues(0);
            }
        }
    }

    int curStatus;

    public void ChangeDOFValues(int i)
    {
        if (i == curStatus)
            return;

        switch(i)
        {
            case 0:
                StartCoroutine(ChangeValues(defValues, false));
                break;
            case 1:
                StartCoroutine(ChangeValues(aimingValues, false));
                break;
            case 2:
                StartCoroutine(ChangeValues(cinematic, false));
                break;
            case 3:
                StartCoroutine(ChangeValues(closeup, false));
                break;
        }

        curStatus = i;
    }

    IEnumerator ChangeValues(DOFvalues v, bool instant)
    {
       /* float curFP = dof.focusPlane;
        float curFR = dof.focusRange;
        float curFS = dof.fStops;

        float targetFP = v.focusPlane;
        float targetFR = v.focusRange;
        float targetFS = v.fstops;

        dof.focusTransform = v.targetTrans;

        float t = 0;

        while(t<1)
        {
            t += Time.deltaTime * 15;

            if (instant)
                t = 1;

            dof.focusPlane = Mathf.Lerp(curFP, targetFP, t);
            dof.focusRange = Mathf.Lerp(curFR, targetFR, t);
            dof.fStops = Mathf.Lerp(curFS, targetFS, t);
            yield return null;
        }*/

        yield return null;
    }

    public static ControlDOF instance;
    public static ControlDOF GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }
}

[System.Serializable]
public class DOFvalues
{
    public float focusPlane;
    public float fstops;
    public float focusRange;
    public Transform targetTrans = null;
}

