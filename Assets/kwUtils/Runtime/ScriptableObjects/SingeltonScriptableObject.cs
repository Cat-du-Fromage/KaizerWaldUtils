using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
//using KaizerWaldCode.PersistentData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static string label = String.Empty;
    
    private static T instance = null;

    protected static T GetInstance(string lbl)
    {
        label = lbl;
        return Instance;
    }

    private static T Instance
    {
        get
        {
            if (instance is not null) return instance;
            AsyncOperationHandle<T> csHandle = Addressables.LoadAssetAsync<T>(label);
            csHandle.WaitForCompletion();
            instance = csHandle.Result;
            return instance;
        }
    }
}
