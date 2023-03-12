using UnityEngine;
using System.Collections;

public class SelectModel_Menu : MonoBehaviour {

    ResourceManager rM;

    public Transform modelButtonsGrid;
    public GameObject buttonPrefab;

    bool init;

    public void Init()
    {
        if (!init)
        {
            rM = ResourceManager.GetInstance();

            for (int i = 1; i < rM.characterModels.Count; i++) //start from 1 since 0 is always the dummy
            {
                GameObject bt = Instantiate(buttonPrefab) as GameObject;
                bt.transform.SetParent(modelButtonsGrid);

                modelSelectButton r = bt.GetComponent<modelSelectButton>();

                r.modelId = rM.characterModels[i].id;
                r.iconText.text = r.modelId;
            }

            init = true;
        }
    }

	
}
