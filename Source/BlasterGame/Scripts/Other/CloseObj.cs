using UnityEngine;
using System.Collections;

namespace Other
{
    public class CloseObj : MonoBehaviour
    {
        public float maxTimer = 1;
        public float timer;

        void Update()
        {
            timer += Time.deltaTime;
            if(timer > maxTimer)
            {
                timer = 0;
                gameObject.SetActive(false);
            }

        }
    }
}
