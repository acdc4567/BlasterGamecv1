using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Other
{
    public class LoadingLevel : MonoBehaviour
    {
        public string text = "Artificial Loading...";
        public Text loadingText;

        void Start()
        {
            StartCoroutine(AnimateText(text));
        }


        IEnumerator AnimateText(string strComplete)
        {
            int i = 0;
            string str = "";
            while (i < strComplete.Length)
            {
                str += strComplete[i++];
                loadingText.text = str;
                yield return new WaitForSeconds(0.08F);
            }

            StartCoroutine(AnimateText(text));
        }
    }
}
