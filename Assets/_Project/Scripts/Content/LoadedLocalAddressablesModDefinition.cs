using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace rwby
{
    public class LoadedLocalAddressablesModDefinition : LoadedModDefinition
    {
        public AsyncOperationHandle<AddressablesModDefinition> handle;

        public override void Unload()
        {
            Addressables.Release(handle);
        }
    }
}