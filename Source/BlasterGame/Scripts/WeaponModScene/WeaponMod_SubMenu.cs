using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class WeaponMod_SubMenu : MonoBehaviour
    {
        List<WeaponMod_Button> buttons = new List<WeaponMod_Button>();
        public Transform grid;

        void CreateButton()
        {
            GameObject go = Instantiate(WeaponModScene.singleton.subMenuButtonPrefab) as GameObject;
            go.transform.SetParent(grid);
            buttons.Add(go.GetComponent<WeaponMod_Button>());
            go.SetActive(false);
        }

        public void Init(List<WModBase> entries)
        {
            Disable();

            for (int i = 0; i < entries.Count; i++)
            {
                if (i > buttons.Count - 1)
                    CreateButton();

                buttons[i].Init(entries[i].id);
                buttons[i].gameObject.SetActive(true);
            }
        }

        public void Disable()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].gameObject.SetActive(false);
            }       
        }  
    }
}