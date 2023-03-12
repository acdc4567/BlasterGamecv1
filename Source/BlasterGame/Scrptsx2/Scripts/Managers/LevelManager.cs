using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

    public int spawnPos;
    public Transform spawnHolder;
    public Transform[] spawnPoints;

    public GameObject playerCharacterPrefab;
    public GameObject cameraHolder;
    
    GameObject playerGo;

    public bool dummy;

	IEnumerator Start () {

        if (!dummy)
        {
            yield return InitializePlayer();
        }
	}

    IEnumerator InitializePlayer()
    {
        spawnPoints = spawnHolder.GetComponentsInChildren<Transform>();

        playerGo = Instantiate(playerCharacterPrefab,
            spawnPoints[spawnPos].position
            , spawnPoints[spawnPos].rotation) as GameObject;

       FreeCameraLook.GetInstance().target = playerGo.transform;

       float startingAngle = Vector3.Angle(Vector3.forward, spawnPoints[spawnPos].forward);
       FreeCameraLook.GetInstance().lookAngle = startingAngle;

       yield return null;
    }

}
