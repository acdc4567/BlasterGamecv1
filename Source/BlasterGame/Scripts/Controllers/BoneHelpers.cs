using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TPC
{
    public class BoneHelpers : MonoBehaviour
    {
        public List<BoneHelper> helpers = new List<BoneHelper>();

        Animator anim;

        public void Init(Animator a)
        {
            anim = a;
        }

        public void Tick()
        {
            if (anim == null)
                anim = GetComponent<Animator>();

            foreach (BoneHelper h in helpers)
            {
                if (h.helper == null)
                    h.helper = ReturnHelper(h.bone).helper;
                
                h.helper.position = anim.GetBoneTransform(h.bone).position;
                h.helper.rotation = anim.GetBoneTransform(h.bone).rotation;
            }
        }

        public void PlaceHelperOnBone(HumanBodyBones b, Animator anim)
        {
            if (anim == null)
                anim = GetComponent<Animator>();

            BoneHelper helper = ReturnHelper(b);
            helper.helper.position = anim.GetBoneTransform(b).position;
            helper.helper.rotation = anim.GetBoneTransform(b).rotation;
        }

        public BoneHelper ReturnHelper(HumanBodyBones b)
        {
            BoneHelper r = null;

            for (int i = 0; i < helpers.Count; i++)
            {
                if (helpers[i].bone == b)
                {
                    r = helpers[i];
                }
            }

            if (r == null)
            {
                BoneHelper n = new BoneHelper();
                n.bone = b;
                GameObject g = new GameObject();
                g.name = b.ToString() + " helper";
                g.transform.parent = this.transform;
                g.transform.localPosition = Vector3.zero;
                g.transform.localEulerAngles = Vector3.zero;
                g.transform.localScale = Vector3.one;
                n.helper = g.transform;
                helpers.Add(n);
                r = n;
            }
            else
            {
                if(r.helper == null)
                {
                    GameObject g = new GameObject();
                    g.name = b.ToString() + " helper";
                    g.transform.parent = this.transform;
                    g.transform.localPosition = Vector3.zero;
                    g.transform.localEulerAngles = Vector3.zero;
                    g.transform.localScale = Vector3.one;
                    r.helper = g.transform;
                }
            }
            

            return r;

        }

        public void ParentAllHelpers()
        {
            foreach (BoneHelper b in helpers)
            {
                Transform p = anim.GetBoneTransform(b.bone).transform;
                b.helper.parent = p;
                b.helper.localPosition = Vector3.zero;
                b.helper.localEulerAngles = Vector3.zero;
            }
        }
    }

    [System.Serializable]
    public class BoneHelper
    {
        public HumanBodyBones bone;
        public Transform helper;
    }
}
