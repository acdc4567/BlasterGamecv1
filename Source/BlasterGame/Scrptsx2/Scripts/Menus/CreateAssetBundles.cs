#if UNITY_EDITOR 
using UnityEditor;

public class CreateAssetBundles {

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    { 
        //BuildPipeline.BuildAssetBundle("Assets/StreamingAssets");
    }
}
#endif