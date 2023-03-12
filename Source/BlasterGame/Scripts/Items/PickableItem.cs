using UnityEngine;
using System.Collections;
using TPC;

namespace TPC.Items
{
    public class PickableItem : MonoBehaviour
    {
        public virtual void PickupItem(StateManager st)
        {
            gameObject.SetActive(false);
            UI.CanvasOverlay.singleton.PickupTextObject.SetActive(false);
        }

        public virtual void OnHighlight(StateManager st)
        {

        }    
    }
}
