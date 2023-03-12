using UnityEngine;
using System.Collections;
using System.IO;

public class LoadAssetLocal : MonoBehaviour {

    public string assetBundleName;

    MainMenu uiM; //changed it to main menu
    ResourceManager rM;

    IEnumerator Start()
    {
        uiM = MainMenu.GetInstance();
        rM = ResourceManager.GetInstance();
        uiM.loading = true;

        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, assetBundleName));
        yield return bundleLoadRequest;

        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            yield break;
        }

        GameObject[] assetLoadRequest = myLoadedAssetBundle.LoadAllAssets<GameObject>(); 
        yield return assetLoadRequest;

        foreach(GameObject g in assetLoadRequest)
        {
            CharacterModels cm = new CharacterModels();
            cm.id = g.name;
            cm.prefab = g;

            rM.characterModels.Add(cm);
        }

        myLoadedAssetBundle.Unload(false);
        
        if (File.Exists(Path.Combine(Application.streamingAssetsPath, assetBundleName + "-mod")))
            yield return CheckForMods();

        uiM.loading = false;
    }

    IEnumerator CheckForMods()
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, assetBundleName+"-mod"));
        yield return bundleLoadRequest;

        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("no mods found");
            yield break;
        }

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

    }
}
