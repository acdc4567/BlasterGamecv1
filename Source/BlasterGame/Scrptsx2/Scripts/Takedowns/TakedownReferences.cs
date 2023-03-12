using UnityEngine;
using System.Collections.Generic;
using System;

public class TakedownReferences : MonoBehaviour {

    public Animator anim;

    public List<BonesList> bonesList = new List<BonesList>();
    public List<RegularMesh> meshList = new List<RegularMesh>();

    public Material blackMaterial;

    public bool initOnStart;

    void Start()
    {
        if (initOnStart)
        {
            Init();
        }
    }

	public void Init () {

        bonesList.Clear();
        meshList.Clear();

        anim = GetComponent<Animator>();

        SkinnedMeshRenderer[] sR = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer s in sR)
        {
            string n = s.name;
            string[] section = n.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

            if (section.Length == 2)
            {
                string p = section[0];
                string id = section[1];

                if (string.Equals(p, "Skl"))
                {
                    BonesList b = new BonesList();
                    b.bone = s.gameObject;
                    b.boneId = id;

                    bonesList.Add(b);
                }
                else
                {
                    RegularMesh mesh = new RegularMesh();

                    mesh.ren = s;
                    mesh.mat = s.material;

                    meshList.Add(mesh);
                }
            }
            else
            {
                RegularMesh mesh = new RegularMesh();

                mesh.ren = s;
                mesh.mat = s.material;

                meshList.Add(mesh);
            }
        }

        CloseSekeleton();
	}

    public void OpenSkeleton()
    {
        foreach (BonesList b in bonesList)
        {
            if (!b.destroyed)
                b.bone.SetActive(true);
        }

        ChangeMaterialsToBlack();
    }

    public void ChangeMaterialsToBlack()
    {
        foreach (RegularMesh m in meshList)
        {
            m.ren.material = blackMaterial;
        }
    }

    public void CloseSekeleton()
    {
        foreach (BonesList b in bonesList)
        {
            b.bone.SetActive(false);
        }

        RevertMaterials();
    }

    public void RevertMaterials()
    {
        foreach (RegularMesh m in meshList)
        {
            m.ren.material = m.mat;
        }
    }

    public void ChangeLayer(int l)
    {
        foreach (RegularMesh m in meshList)
        {
            m.ren.gameObject.layer = l;
        }

        MeshRenderer[] storeMeshes = transform.GetComponentsInChildren<MeshRenderer>();

        foreach(MeshRenderer m in storeMeshes)
        {
            m.gameObject.layer = l;
        }
    }
}

[System.Serializable]
public class BonesList
{
    public string boneId;
    public GameObject bone;
    public bool destroyed;
}

[System.Serializable]
public class RegularMesh
{
    public SkinnedMeshRenderer ren;
    public Material mat;
}
