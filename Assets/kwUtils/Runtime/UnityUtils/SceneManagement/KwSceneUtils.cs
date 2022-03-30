using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class KwSceneUtils
{
    public enum KwScene
    {
        MainMenu,
        Game,
    }

    private static Dictionary<KwScene ,SceneInstance> sceneInstances;

    static KwSceneUtils()
    {
#pragma warning disable CS4014
        GetAll("SceneAsset");
#pragma warning restore CS4014
    }
    
    private static async Task GetAll(string label)
    {
        IList<IResourceLocation> unloadedLocations = await Addressables.LoadResourceLocationsAsync(label).Task;

        foreach (IResourceLocation location in unloadedLocations)
        {
            Register(GetKwSceneByString(location.PrimaryKey), (SceneInstance)location.Data);
        }
    }

    private static KwScene GetKwSceneByString(string str)
    {
        Array sceneObjects = Enum.GetValues(typeof(KwScene));
        
        for (int i = 0; i < sceneObjects.Length - 1; i++)
        {
            KwScene sceneObj = (KwScene)sceneObjects.GetValue(i);
            if (sceneObj.ToString().Equals(str))
            {
                Debug.Log($"Length Register == {sceneObj.ToString()}");
                return sceneObj;
            }
        }
        return KwScene.MainMenu;
    }
    
    private static void Register(KwScene scene, SceneInstance sceneInstance) => sceneInstances.Add(scene,sceneInstance);

    private static void UnloadScene(AsyncOperationHandle<SceneInstance> currentSi)
    {
        Addressables.UnloadSceneAsync(currentSi, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
    }
    
    public static AsyncOperationHandle<SceneInstance> Load(KwScene scene)
    {
        return Addressables.LoadSceneAsync(scene.ToString());
    }
    
}
