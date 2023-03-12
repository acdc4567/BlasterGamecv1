using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour {

    public int activeModelIndex = 1;

    public List<CharacterModels> characterModels = new List<CharacterModels>();

	public void SwitchCharacterModelWithIndex(StateManager st,int target)
    {
        StartCoroutine(SwitchCharacter(st, target));
    }

    public void SwitchCharacterModelWithId(StateManager st, string id)
    {
        int index = ReturnCharacterModelIndexFromId(id);

        StartCoroutine(SwitchCharacter(st, index));
    }

    IEnumerator SwitchCharacter(StateManager st, int target)
    {
        yield return SwitchCharacterWith(st, 0);
        yield return SwitchCharacterWith(st, target);
    }

    IEnumerator SwitchCharacterWith(StateManager st, int target)
    {
        if (!st.model.activeInHierarchy)
            st.model.SetActive(true);

        List<ShareableObj> getAllObjs = st.weaponManager.PopulateAndReturnShareableList();

        List<ShareableAssetsInfo> l = new List<ShareableAssetsInfo>();

        foreach (ShareableObj o in getAllObjs)
        {
            ShareableAssetsInfo n = new ShareableAssetsInfo();
            n.obj = o.gameObject;
            n.pos = o.transform.localPosition;
            n.rot = o.transform.localRotation;
            //n.scale = o.transform.localScale;
            n.parentBone = o.parentBone;
            n.wasActive = n.obj.activeInHierarchy;
            n.obj.SetActive(false);
            o.transform.parent = null;
            n.resetScale = o.resetScale;
            n.zeroin = o.zeroIn;
            l.Add(n);

        }

        GameObject newModel = Instantiate(characterModels[target].prefab, Vector3.zero, Quaternion.identity) as GameObject;

        newModel.transform.parent = st.transform;
        newModel.transform.localPosition = Vector3.zero;
        newModel.transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (target == 0)
            newModel.SetActive(false);

        GameObject prevModel = st.model;
        st.model = newModel;
        st.handleAnim.SetupAnimator(newModel.GetComponent<Animator>());

        Destroy(prevModel);

        st.handleAnim.anim.Rebind();

        for (int i = 0; i < l.Count; i++)
        {
            Transform t = l[i].obj.transform;

            t.parent = st.handleAnim.anim.GetBoneTransform(l[i].parentBone);
            t.localPosition = l[i].pos;
            t.localRotation = l[i].rot;
            //t.localScale = l[i].scale;

            if(l[i].resetScale)
            {
                t.localScale = Vector3.one;
            }

            if (l[i].zeroin)
            {
                t.localPosition = Vector3.zero;
                t.localEulerAngles = Vector3.zero;
            }

            l[i].obj.SetActive(l[i].wasActive);
        }

        activeModelIndex = target;

        st.ChangedModelCallBack();

        yield return null;
    }

    int ReturnCharacterModelIndexFromId(string id)
    {
        int retVal = 0;

        for (int i = 0; i < characterModels.Count; i++)
        {
            if (string.Equals(characterModels[i].id, id))
            {
                retVal = i;
                break;
            }
        }

        return retVal;
    }

    public static ResourceManager instance;
    public static ResourceManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
}

[System.Serializable]
public class CharacterModels
{
    public string id;
    public GameObject prefab;
}


[System.Serializable]
public class ShareableAssetsInfo
{
    public GameObject obj;
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public bool wasActive;
    public HumanBodyBones parentBone;
    public bool resetScale;
    public bool zeroin;

}

