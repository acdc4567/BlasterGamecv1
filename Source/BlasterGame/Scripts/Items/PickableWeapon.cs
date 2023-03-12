using UnityEngine;
using System.Collections;
using Weapons;

namespace TPC.Items
{
    public class PickableWeapon : PickableItem
    {
        public string weaponId;
        public int curBullets = 30;
        public int carryBullets = 160;

        public override void PickupItem(StateManager st)
        {
            st.weaponManager.PickupWeapon(weaponId, curBullets, carryBullets);
            base.PickupItem(st);
            
        }

        public override void OnHighlight(StateManager st)
        {
            
        }

        void OnTriggerEnter(Collider other)
        {
            InputHandler inp = other.GetComponent<InputHandler>();

            if (inp != null)
            {
                OnHighlight(inp.states);
                inp.CanPickupItem(this);
                UI.CanvasOverlay.singleton.pickupText.text = weaponId.ToUpper();
                UI.CanvasOverlay.singleton.PickupTextObject.SetActive(true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            InputHandler inp = other.GetComponent<InputHandler>();

            if (inp != null)
            {
                inp.DisablePickupItem();
                UI.CanvasOverlay.singleton.PickupTextObject.SetActive(false);
            }
        }

    }
}
