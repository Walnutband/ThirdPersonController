using UnityEngine.AddressableAssets;

/// <summary>
/// A utility class for various Addressables functionality
/// </summary>
public static class AddressablesUtility
{
    /// <summary>
    /// Get the address of a given AssetReference.
    /// </summary>
    /// <param name="reference">The AssetReference you want to find the address of.</param>
    /// <returns>The address of a given AssetReference.</returns>
    public static string GetAddressFromAssetReference(AssetReference reference)
    {
        /*这行代码启动了一个异步操作，请求加载与给定 AssetReference 相关的所有资源位置（即资源在 Addressables 系统中对应的物理或逻辑位置）。
        正常情况下，该异步操作会在后台运行，并在加载完成后通过回调或通过 Completed 事件通知。*/
        var loadResourceLocations = Addressables.LoadResourceLocationsAsync(reference);
        /*这里调用 WaitForCompletion() 会造成当前线程（通常是主线程）阻塞，直到异步操作完成为止。也就是说，虽然底层 API 是异步的，但通过这种方式可以在调用处立即获得结果，而不需要通过事件或 await 等方式等待回调。*/
        var result = loadResourceLocations.WaitForCompletion();
        
        if (result.Count > 0)
        {
            string key = result[0].PrimaryKey;
            Addressables.Release(loadResourceLocations);
            return key;
        }

        Addressables.Release(loadResourceLocations);
        return string.Empty;
    }
}
