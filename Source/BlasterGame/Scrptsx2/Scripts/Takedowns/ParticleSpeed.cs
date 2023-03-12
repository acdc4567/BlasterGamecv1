using UnityEngine;
using System.Collections;

public class ParticleSpeed : MonoBehaviour {

    TimeManager tM;

    ParticleSystem[] ps;

    void Start()
    {
        tM = TimeManager.GetInstance();
        ps = GetComponentsInChildren<ParticleSystem>();
    }
	
	void Update () {

        for (int i = 0; i < ps.Length; i++)
        {
            ps[i].playbackSpeed = tM.myTimeScale;
        }

	}
}
