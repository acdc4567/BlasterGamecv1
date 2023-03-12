using UnityEngine;
using System.Collections;

[System.Serializable]
public class Item {

    public string itemId;
    public GameObject modelPrefab;
    public HumanBodyBones bone;
    public Vector3 localPosition;
    public Vector3 localEuler;
    public Vector3 localScale;
}
