using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesFighterDefinition", menuName = "rwby/Content/Addressables/FighterDefinition")]
    public class AddressablesFighterDefinition : IFighterDefinition
    {
        public override string Name { get { return fighterName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }
        public override int Health { get { return health; } }
        public override int Aura
        {
            get { return aura; }
        }

        public override int AuraGainPerFrame
        {
            get { return auraGainPerFrame; }
        }

        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReferenceT<GameObject> fighterReference;
        [SerializeField] private AssetReferenceT<Moveset>[] movesetReferences;
        [SerializeField] private bool selectable = true;
        [SerializeField] private int health;
        [SerializeField] private int aura;
        [SerializeField] private int auraGainPerFrame;

        [NonSerialized] private AsyncOperationHandle<Moveset>[] movesetHandles;
        [NonSerialized] private AsyncOperationHandle<GameObject> fighterHandle;

        public override async UniTask<bool> Load()
        {
            if (fighterHandle.IsValid() && fighterHandle.Status == AsyncOperationStatus.Succeeded) return true;

            // Load fighter.
            try
            {
                fighterHandle = Addressables.LoadAssetAsync<GameObject>(fighterReference);
                await fighterHandle;
                bool fighterRequirementsResult = await fighterHandle.Result.GetComponent<FighterManager>().OnFighterLoaded();
                if (fighterRequirementsResult == false)
                {
                    Debug.LogError($"Error loading fighter {Name}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading fighter {Name}: {e.Message}");
                return false;
            }


            // Load movesets.
            try
            {
                movesetHandles = new AsyncOperationHandle<Moveset>[movesetReferences.Length];
                for (int i = 0; i < movesetReferences.Length; i++)
                {
                    var handle = Addressables.LoadAssetAsync<Moveset>(movesetReferences[i]);
                    movesetHandles[i] = handle;
                    await movesetHandles[i];
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            return true;
        }

        public override GameObject GetFighter()
        {
            return fighterHandle.Result;
        }

        public override string GetFighterGUID()
        {
            return fighterReference.AssetGUID;
        }

        
        public override Moveset[] GetMovesets()
        {
            Moveset[] m = new Moveset[movesetHandles.Length];
            for (int i = 0; i < movesetHandles.Length; i++)
            {
                m[i] = movesetHandles[i].Result;
            }
            return m;
        }

        public override bool Unload()
        {
            foreach (var t in movesetHandles)
            {
                if(t.Status == AsyncOperationStatus.Succeeded) Addressables.Release(t);
            }
            if(fighterHandle.Status == AsyncOperationStatus.Succeeded) Addressables.Release(fighterReference);
            return true;
        }
    }
}