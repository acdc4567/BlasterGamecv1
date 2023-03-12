using UnityEngine;
using System.Collections.Generic;

namespace Manager
{
    public class AudioManager : MonoBehaviour
    {
        public List<AudioFX> audioFx = new List<AudioFX>();
        Dictionary<string, int> aIndex = new Dictionary<string, int>();

        public AudioFX GetAudio(string key)
        {
            AudioFX r = null;
            int index = -1;

            if(aIndex.TryGetValue(key,out index))
            {
                r = audioFx[index];
            }

            return r;
        }

        void Start()
        {
            for (int i = 0; i < audioFx.Count; i++)
            {
                if(aIndex.ContainsKey(audioFx[i].clipId))
                {
                    Debug.Log("Multiple audio clips using the same id! This is not allowed");
                    continue;
                }

                aIndex.Add(audioFx[i].clipId, i);
            }
        }

        static public AudioManager singleton;
        void Awake()
        {
            singleton = this;
        }

    }

    [System.Serializable]
    public class AudioFX
    {
        public string clipId;
        public AudioClip audioClip;
    }
}