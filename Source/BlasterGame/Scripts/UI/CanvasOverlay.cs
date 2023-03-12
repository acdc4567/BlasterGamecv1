using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI
{
    public class CanvasOverlay : MonoBehaviour
    {
        public bool goToObjective;
        public Transform goToTarget;
        public Transform goToIndicator;

        public GameObject objectivesObject;
        public GameObject objComplete;
        public Text objText;
        public RectTransform completeObjMask;
        public RectTransform objectiveMask;
        public RectTransform radarObject;
        public GameObject objectiveUIs;
        public GameObject PickupTextObject;
        public Text pickupText;

        void Start()
        {
            objectivesObject.SetActive(false);
            objComplete.SetActive(false);
            PickupTextObject.SetActive(false);

            if(Manager.SessionMaster.singleton.debugMode)
            {
                objectiveUIs.SetActive(false);
            }
        }

        void Update()
        {
            if (goToObjective)
            {
                if (goToTarget != null)
                    FollowObject();
                else
                    goToObjective = false;
            }

            Vector3 e = Camera.main.transform.eulerAngles;
            Vector3 d = Vector3.zero;
            d.z = e.y;
            radarObject.eulerAngles = d;
        }

        void FollowObject()
        {
            if (PointInsideCamera(goToTarget.position))
            {
                goToIndicator.gameObject.SetActive(true);
                Vector3 targetPos = Camera.main.WorldToScreenPoint(goToTarget.position);
                goToIndicator.position = targetPos;
            }
            else
            {
                goToIndicator.gameObject.SetActive(false);
            }
        }

        public bool PointInsideCamera(Vector3 point)
        {
            bool r = true;
            Vector3 camView = Camera.main.WorldToViewportPoint(point);

            if (camView.x < 0 || camView.x > 1 || camView.y < 0 || camView.y > 1 || camView.z < 0)
            {
                r = false;
            }

            return r;
        }

        public void OpenObjective(string obj, bool complete)
        {
            StartCoroutine(UpdateObjective(obj, complete));
        }

        IEnumerator UpdateObjective(string obj, bool complete)
        {
            objectivesObject.SetActive(false);
            if (complete)
            {
                objComplete.SetActive(true);
                yield return ObjectiveText("Objective Complete", completeObjMask);
                yield return new WaitForSeconds(1.5f);
            }
            objectivesObject.SetActive(true);
            yield return ObjectiveText(obj, objectiveMask);
            yield return new WaitForSeconds(1.5f);
            objComplete.SetActive(false);
        }

        IEnumerator ObjectiveText(string obj, RectTransform targetRect)
        {
            objText.text = obj;
            Vector2 size = targetRect.sizeDelta;
            size.x = 0;
            targetRect.sizeDelta = size;

            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime;

                float x = Mathf.Lerp(0, 500, t);
                size.x = x;
                targetRect.sizeDelta = size;
                yield return null;
            }
        }

        static public CanvasOverlay singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
