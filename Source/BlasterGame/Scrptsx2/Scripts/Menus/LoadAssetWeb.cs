using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LoadAssetWeb : MonoBehaviour {

    public string assetBundleName;

    MainMenu uiM;
    ResourceManager rM;

    IEnumerator Start()
    {
        rM = ResourceManager.GetInstance();
        uiM = MainMenu.GetInstance();

        uiM.loading = true;
      
        //you need to upload the asset bundles in your servers as a binary file
        //and add the url for the bundle (not the .manifest!) here
        //this will always download the files, you can use WWW.LoadFromCacheOrDownload instead
        //https://docs.unity3d.com/ScriptReference/WWW.LoadFromCacheOrDownload.html

        using (WWW www = new WWW("http://yourserverhere"))
        {
            yield return www;
            
            if (www.error != null)
                throw new Exception("WWW download had an error:" + www.error);

            AssetBundle myLoadedAssetBundle = www.assetBundle;

            if (myLoadedAssetBundle == null)
                Debug.Log("No bundle found");

            GameObject[] assetLoadRequest = myLoadedAssetBundle.LoadAllAssets<GameObject>();
            yield return assetLoadRequest;

            foreach (GameObject g in assetLoadRequest)
            {
                CharacterModels cm = new CharacterModels();
                cm.id = g.name;
                cm.prefab = g;

                rM.characterModels.Add(cm);
            }

            myLoadedAssetBundle.Unload(false);

            uiM.loading = false;        
        } 
    }

}
