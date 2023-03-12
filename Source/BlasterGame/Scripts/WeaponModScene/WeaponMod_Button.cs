using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Weapons.Modifications
{
    public class WeaponMod_Button : MonoBehaviour
    {
        public string modId;
        public WModType type;
        public Text txt;

        public void Init(string id)
        {
            if(txt == null)
                txt = GetComponentInChildren<Text>();

            txt.text = id;
            modId = id;
        }

        public void Press()
        {
            WeaponModScene.singleton.SelectMod(modId);
        }
    }
}
