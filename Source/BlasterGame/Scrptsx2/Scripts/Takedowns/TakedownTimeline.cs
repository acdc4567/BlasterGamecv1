using UnityEngine;
using System.Collections;

public class TakedownTimeline : MonoBehaviour {

    public string timelineName;
    public Takedown tD;
    TakedownCinematic main;

    [Range(0, 1)]
    public float perc;
    float delta;

    Vector3 targetPos;
    Transform targetTrans;

    public bool cinematicTakedown;
    public bool jumpCut;

    public WeaponReferenceBase takedownWeapon;

    public void Init(TakedownCinematic m, TakedownReferences pl, TakedownReferences en)
    {
        tD.player = pl;
        tD.enemy = en;

        if(main == null)
        {
            main = m;
            
            if(!cinematicTakedown)
            {
                tD.xRay = false;
            }
        }
    }

    public void Tick()
    {
        if (!tD.xRay)
            if (tD.c_xRay)
                tD.c_xRay.gameObject.SetActive(false);

        if (targetTrans)
            targetPos = targetTrans.position;

        main.camHelper.position = Vector3.Lerp(main.camHelper.position,
            targetPos, Time.deltaTime * 5);

        delta = main.tM.GetDelta();

        tD.player.anim.speed = main.tM.myTimeScale;
        tD.enemy.anim.speed = main.tM.myTimeScale;
        tD.cameraAnim.speed = main.tM.myTimeScale;

        perc += delta / tD.totalLength;

        if (perc > 1)
            perc = 1;

        if (perc < 0)
            perc = 0;

        Vector3 camPos = tD.camPath.GetPointAt(perc);
  
        if (camPos != Vector3.zero)
            tD.cameraRig.transform.position = camPos;

        tD.cameraRig.LookAt(main.camHelper);
    }

    IEnumerator ChangeTimeScale(float targetScale)
    {
        float target = targetScale;
        float cur = main.tM.myTimeScale;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 15;

            float v = Mathf.Lerp(cur, target, t);

            if (cinematicTakedown)
            {
                main.tM.myTimeScale = v;
            }

            yield return null;
        }

    }

    IEnumerator ChangeFOV(float targetValue)
    {
        float target = targetValue;
        float curValue = Camera.main.fieldOfView;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 15;

            float v = Mathf.Lerp(curValue, target, t);

            if (cinematicTakedown)
            {
                Camera.main.fieldOfView = v;

                if (tD.xRay)
                    tD.c_xRay.fieldOfView = v;
            }

            yield return null;
        }
    }

    public void PlayEvent(string e_name)
    {
        Invoke(e_name,0);
    }

    public void BreakBone(string b_name)
    {
        BonesList bone = ReturnBone(b_name);

        if (bone != null)
        {
            bone.bone.SetActive(false);
            bone.destroyed = true;
        }

        PlayParticle();
    }

    public void PlayParticle(int i = 0)
    {
        foreach (ParticleSystem ps in tD.particles[i].particles)
        {       
            if(tD.xRay)
            {
                ps.gameObject.layer = main.xrayLayer;
            }
            else
            {
                ps.gameObject.layer = main.defaultLayer;
            }

            ps.Play();
        }
        
    }

    public void ChangeDOF(int i)
    {
        if(ControlDOF.GetInstance())
        {
            ControlDOF.GetInstance().ChangeDOFValues(i);
        }
    }

    BonesList ReturnBone(string target)
    {
        BonesList retVal = null;

        for (int i = 0; i < tD.enemy.bonesList.Count; i++)
        {
            if (string.Equals(tD.enemy.bonesList[i].boneId, target))
            {
                retVal = tD.enemy.bonesList[i];
            }
        }

        return retVal;
    }

    public void ChangeCameraTarget(int i)
    {
        Takedown_CamTargets tInfo = tD.camT[i];

        if(tInfo.assignBone)
        {
            if(tInfo.fromPlayer)
            {
                targetTrans = tD.player.anim.GetBoneTransform(tInfo.bone);
            }
            else
            {
                targetTrans = tD.enemy.anim.GetBoneTransform(tInfo.bone);
            }
        }
        else
        {
            targetTrans = tInfo.target;
        }

        if(tInfo.jumpTo)
        {
            main.camHelper.position = targetTrans.position;
        }
    }

    void OpenSkeleton_Enemy()
    {
        if (tD.xRay)
        {
            tD.enemy.OpenSkeleton();

            tD.player.ChangeLayer(main.xrayLayer);
        }
    }

    void CloseSkeletons()
    {
        tD.enemy.CloseSekeleton();
        tD.player.CloseSekeleton();
        tD.player.ChangeLayer(main.defaultLayer);
    }
}
