using UnityEngine;
using System.Collections;
using Manager.Objectives;
using Weapons;

namespace TPC.Items
{
    public class PickableAmmo : PickableItem
    {
        public string weaponId;
        public int value;
        bool compatible;

        public override void PickupItem(StateManager st)
        {
            if(!compatible)
                return;

            WeaponManager wm = st.weaponManager;
            RuntimeWeapon rw = wm.weaponReferences[0];
            if (rw.wReference.weaponId != weaponId)
                rw = wm.weaponReferences[1];

            if (rw.wReference.weaponId != weaponId)
                return;

            rw.activeStats.LoadBullets(value);

            base.PickupItem(st);
        }

        public override void OnHighlight(StateManager st)
        {
            WeaponManager wm = st.weaponManager;
            bool isCompatible = false;

            for (int i = 0; i < wm.weapons.Count; i++)
            {
                if(string.Equals(weaponId,wm.weapons[i]))
                {
                    //Compatible
                    isCompatible = true;
                    break;
                }
            }

            compatible = isCompatible;
        }

        void OnTriggerEnter(Collider other)
        {
            InputHandler inp = other.GetComponent<InputHandler>();

            if (inp != null)
            {
                OnHighlight(inp.states);
                inp.CanPickupItem(this);
                UI.CanvasOverlay.singleton.pickupText.text = weaponId.ToUpper() + " AMMO";
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
