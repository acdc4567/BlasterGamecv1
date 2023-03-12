using UnityEngine;
using System.Collections;

public class CharacterAudioManager : MonoBehaviour {

    public AudioSource gunSounds;
    public AudioSource runFoley;

    public float footStepTimer;
    public float walkThreshold;
    public float runThreshold;
    public AudioSource footStep1;
    public AudioSource footStep2;
    public AudioClip[] footStepClips;
    public AudioSource effectsSource;
    public AudioClipsList[] effectsList;
    StateManager states;
    
    float startingVolumeRun;
    float characterMovement;

	public void Init() {
        states = GetComponent<StateManager>();
        startingVolumeRun = runFoley.volume;

        runFoley.volume = 0;
	}

    public void Tick() 
    {
        characterMovement = Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical);
        characterMovement = Mathf.Clamp01(characterMovement);

        float targetThreshold = 0;

        if (!states.walk && !states.aiming && !states.reloading && !states.down && !states.crouching)
        {
            runFoley.volume = startingVolumeRun * characterMovement;
            targetThreshold = runThreshold;
        }     
        else
        {
            targetThreshold = walkThreshold;            
            runFoley.volume = Mathf.Lerp(runFoley.volume, 0, Time.deltaTime * 2);
        }
        
        if (characterMovement > 0)
        {
            footStepTimer += Time.deltaTime;

            if(footStepTimer > targetThreshold)
            {
                if (!states.down)
                { PlayFootStep(); }
                footStepTimer = 0;
            }
        }
        else
        {
            footStep1.Stop();
            footStep2.Stop();
        }
	}


    public void PlayGunSound()
    {
        gunSounds.Play();
    }

    public void PlayFootStep()
    {
        if (!footStep1.isPlaying)
        {
            int ran = Random.Range(0, footStepClips.Length);
            footStep1.clip = footStepClips[ran];

            footStep1.Play();
        }
        else
        {
            if (!footStep2.isPlaying)
            {
                int ran2 = Random.Range(0, footStepClips.Length);
                footStep2.clip = footStepClips[ran2];
                footStep2.Play();
            }
        }
    }

    public void PlayEffect(string name)
    {
        AudioClip clip = null;

        for (int i = 0; i < effectsList.Length; i++)
        {
            if(string.Equals(effectsList[i].name, name))
            {
                clip = effectsList[i].clip;
                break;
            }
        }

        effectsSource.clip = clip;
        effectsSource.Play();
    }

}

[System.Serializable]
public class AudioClipsList
{
    public string name;
    public AudioClip clip;
}
