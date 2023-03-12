using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Manager;

namespace UI
{
    public class SelectCharacterButton : MonoBehaviour
    {
        string charId;
        Text txt;

        public void Init(string id)
        {
            txt = GetComponentInChildren<Text>();
            txt.text = id;
            charId = id;
        }

        public void SelectCharacter()
        {
            MainMenu_Manager.singleton.targetCharId = charId;
            MainMenu_Manager.singleton.LoadCharacter();
        } 
    }
}
