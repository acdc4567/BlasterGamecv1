using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour {

    float myDelta;
    float myFixedDelta;
    public float myTimeScale = 1;
    
    void Start()
    {
        myTimeScale = 1;
    }

	void FixedUpdate()
    {
        myFixedDelta = Time.fixedDeltaTime * myTimeScale;
    }

	void Update () 
    {
        myDelta = Time.deltaTime * myTimeScale;
	}

    public float GetDelta()
    {
        return myDelta;
    }

    public float GetFixDelta()
    {
        return myFixedDelta;
    }

    public static TimeManager instance;
    public static TimeManager GetInstance()
    {
        if(instance == null)
        {
            GameObject go = new GameObject();
            go.name = "time manager";
            go.AddComponent<TimeManager>();
            instance = go.GetComponent<TimeManager>();
        }

        return instance;
    }

    void Awake()
    {
        instance = this;
    }
}
