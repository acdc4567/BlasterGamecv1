using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class modelSelectButton : MonoBehaviour {

    public string modelId;
    public Text iconText;

    MainMenu m;
    
    void Awake()
    {
        m = MainMenu.GetInstance();
    }

    public void SwitchModel()
    {
        m.SwitchModel(modelId);
    }

}
