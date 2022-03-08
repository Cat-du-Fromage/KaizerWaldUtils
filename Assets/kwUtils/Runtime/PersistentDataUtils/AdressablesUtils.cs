using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace KaizerWaldCode.Utils
{
    public static class AddressablesUtils
    {
        public static AsyncOperationHandle<T> LoadSingleAssetSync<T>(AssetReference assetRef) where T : Object
        {
            AsyncOperationHandle<T> csHandle = Addressables.LoadAssetAsync<T>(assetRef);
            csHandle.WaitForCompletion();
            return csHandle;
        }
        
        public static AsyncOperationHandle<T> LoadSingleAssetSync<T>(string label)
        {
            AsyncOperationHandle<T> csHandle = Addressables.LoadAssetAsync<T>(label);
            csHandle.WaitForCompletion();
            return csHandle;
        }

        public static AsyncOperationHandle<GameObject> InstanciateSingleAssetSync(AssetReference assetRef)
        {
            AsyncOperationHandle<GameObject> csHandle = Addressables.InstantiateAsync(assetRef);
            csHandle.WaitForCompletion();
            return csHandle;
        }

        public static async Task CreateAssetAddToList<T>(AssetReference reference, List<T> completedObjs)
            where T : Object
        {
            completedObjs.Add(await reference.InstantiateAsync().Task as T);
        }

        public static async Task CreateAssetsAddToList<T>(List<AssetReference> references, List<T> completedObjs)
            where T : Object
        {
            foreach (AssetReference reference in references)
                completedObjs.Add(await reference.InstantiateAsync().Task as T);
        }

        public static async Task GetAll(string label, IList<IResourceLocation> loadedLocations)
        {
            IList<IResourceLocation> unloadedLocations = await Addressables.LoadResourceLocationsAsync(label).Task;
            foreach (IResourceLocation location in unloadedLocations)
                loadedLocations.Add(location);
        }
    }

}
