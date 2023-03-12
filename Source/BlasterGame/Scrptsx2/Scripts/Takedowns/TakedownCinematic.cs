using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TakedownCinematic : MonoBehaviour {

    [HideInInspector]
    public TimeManager tM;

    public bool runTakedown;
    bool initTakedown;

    public int t_index;
    public bool xray;
    public List<TakedownHolder> takedownList = new List<TakedownHolder>();

    public Transform camHelper;

    TakedownTimeline curTimeline;

    public bool debugMode;
    public int debugTD;

    public int xrayLayer;
    public int defaultLayer;

    public GameObject mainCameraRig;
    public Transform mainCamera;
    float curFov;

    public TakedownReferences playerRef;
    public TakedownReferences enemyRef;

    Vector3 storeCamPosition;

    void Start()
    {
        xrayLayer = 11;

        tM = TimeManager.GetInstance();

        camHelper = new GameObject().transform;

        InitTakedownHolders();

        if (debugMode)
        {
            playerRef.Init();
            enemyRef.Init();
        }

    }

    void Update()
    {
        if(runTakedown)
        {
            Takedown t = takedownList[t_index].timeline.tD;
            
            curTimeline = takedownList[t_index].timeline;
            InitTakedown(takedownList[t_index]);
            curTimeline.tD.xRay = xray;
            curTimeline.Init(this,playerRef,enemyRef);

            if(!initTakedown)
            {
                curTimeline.ChangeCameraTarget(0);

                InitParticles(t);

                t.enemy.transform.rotation = t.player.transform.rotation;
                Vector3 worldPos = t.enemy.transform.TransformDirection(t.info.offset);
                worldPos += t.enemy.transform.position;

                StartCoroutine(
                    LerpToTargetPos_andPlayAnims(
                    worldPos,t
                    ));

                initTakedown = true;
            } 
            else
            {
                if(curTimeline)
                {
                    if(curTimeline.cinematicTakedown)
                        curTimeline.Tick();
                }
            }
        }
    }

    IEnumerator LerpToTargetPos_andPlayAnims(Vector3 targetPos, Takedown _t)
    {
        Vector3 dest = targetPos;
        Vector3 from = _t.player.transform.position;

        float perc = 0;

        while (perc < 1)
        {
            if (curTimeline.jumpCut)
                perc = 1;
            else
                perc += Time.deltaTime * 5;

            _t.player.transform.position = Vector3.Lerp(from, dest, perc);

           // Vector3 lp = _t.enemy.transform.position - _t.player.transform.position;
           // lp.y = 0;

            //_t.player.transform.LookAt(lp);

            yield return null;
        }

        _t.cameraAnim.enabled = true;
        _t.cameraAnim.Play(curTimeline.timelineName);

        _t.player.anim.CrossFade( _t.info.p_anim,
                                    _t.info.p_crossfade_timer);

        yield return new WaitForSeconds(_t.info.e_delay);

        _t.enemy.anim.CrossFade(_t.info.e_anim,
                                    _t.info.e_crossfade_timer);
    }

    void InitParticles(Takedown t)
    {
        for (int i = 0; i < t.particles.Length; i++)
        {
            ParticlesForTakedowns p = t.particles[i];

            GameObject go = p.particleGO;

            if (go == null)
            {
                    go = Instantiate(p.particlePrefab,
                    transform.position,
                    Quaternion.identity) as GameObject;
            }

            if(p.particles.Length ==0)
            {
                p.particles = go.GetComponentsInChildren<ParticleSystem>();
            }

            p.particleGO = go;

            if(p.placeOnBone)
            {
                p.targetTrans = (p.playerBone) ? 
                    t.player.anim.GetBoneTransform(p.bone) :
                    t.enemy.anim.GetBoneTransform(p.bone);
            }

            p.particleGO.transform.parent = p.targetTrans;
            p.particleGO.transform.localPosition = p.targetPos;
            p.particleGO.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    void InitTakedownHolders()
    {
        for (int i = 0; i < takedownList.Count; i++)
        {
            TakedownHolder t = takedownList[i];

            t.timeline = t.holder.GetComponentInChildren<TakedownTimeline>();

            t.holder.SetActive(false);
        }
    }
    
    void InitTakedown(TakedownHolder th)
    { 
        if(th.timeline.cinematicTakedown)
        {
            Cursor.lockState = CursorLockMode.Confined;

            storeCamPosition = mainCamera.transform.localPosition;
            mainCamera.transform.parent = curTimeline.transform;
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

            if(curTimeline.tD.xRay)
            {
                Transform xray = CameraReferences.GetInstance().xray.transform;

                xray.parent = curTimeline.transform;
                xray.localPosition = Vector3.zero;
                xray.localRotation = Quaternion.Euler(Vector3.zero);
                xray.gameObject.SetActive(true);

                curTimeline.tD.c_xRay = CameraReferences.GetInstance().xray;
            }

            mainCameraRig.SetActive(false);

        }

        th.holder.SetActive(true);
    }

    public void CloseTakedown()
    {
        mainCameraRig.SetActive(true);
        
        Camera.main.fieldOfView = 60;
        CameraReferences.GetInstance().xray.fieldOfView = 60;

        mainCamera.transform.parent = FreeCameraLook.GetInstance().pivot.GetChild(0);
        mainCamera.transform.localPosition = storeCamPosition;
        mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

        CameraReferences.GetInstance().xray.transform.parent = FreeCameraLook.GetInstance().pivot.GetChild(0); ;
        CameraReferences.GetInstance().xray.transform.localPosition = Vector3.zero;
        CameraReferences.GetInstance().xray.transform.localRotation = Quaternion.Euler(Vector3.zero);

        CameraReferences.GetInstance().xray.gameObject.SetActive(false);
        CloseAllTakedowns();
        initTakedown = false;
        runTakedown = false;
        curTimeline.perc = 0;
        curTimeline = null;
    }

    void CloseAllTakedowns()
    {
        foreach (TakedownHolder t in takedownList)
        {
            t.holder.SetActive(false);
        }
    }
}

[System.Serializable]
public class TakedownHolder
{
    public string id;
    public TakedownTimeline timeline;
    public GameObject holder;
}

[System.Serializable]
public class Takedown
{
    public string id;
    public float totalLength;
    public Takedown_Info info;
    public Animator cameraAnim;
    public BezierCurve camPath;
    public TakedownReferences player;
    public TakedownReferences enemy;
    [HideInInspector]
    public int cam_t_index;
    public Takedown_CamTargets[] camT;
    public bool xRay;
    public Transform cameraRig;
    public Camera c_xRay;
    public ParticlesForTakedowns[] particles;
}

[System.Serializable]
public class Takedown_Info
{
    public string p_anim;
    public float p_crossfade_timer = 0.2f;
    public string e_anim;
    public float e_crossfade_timer = 0.2f;
    public float e_delay;
    public Vector3 offset;
}

[System.Serializable]
public class Takedown_CamTargets
{
    public Transform target;
    public bool assignBone = true;
    public HumanBodyBones bone;
    public bool fromPlayer;
    public bool jumpTo;
}

[System.Serializable]
public class ParticlesForTakedowns
{
    public GameObject particlePrefab;
    [HideInInspector]
    public GameObject particleGO;
    public bool placeOnBone;
    public bool playerBone;
    public HumanBodyBones bone;
    public Transform targetTrans;
    [HideInInspector]
    public ParticleSystem[] particles;
    public Vector3 targetPos;
    public Vector3 targetRot;
}