using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesFighterDefinition", menuName = "rwby/Content/Addressables/FighterDefinition")]
    public class AddressablesFighterDefinition : IFighterDefinition
    {
        public override string Name { get { return fighterName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }
        public override int Health { get { return health; } }

        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReferenceT<GameObject> fighterReference;
        [SerializeField] private AssetReferenceT<Moveset>[] movesetReferences;
        [SerializeField] private bool selectable = true;
        [SerializeField] private int health;

        [NonSerialized] private Moveset[] movesets = null;
        [NonSerialized] private GameObject fighter = null;

        public override async UniTask<bool> Load()
        {
            if (fighter != null)
            {
                return true;
            }

            // Load fighter.
            try
            {
                OperationResult<GameObject> fighterLoadResult = await AddressablesManager.LoadAssetAsync(fighterReference);
                fighter = fighterLoadResult.Value;
                bool fighterRequirementsResult = await fighter.GetComponent<FighterManager>().OnFighterLoaded();
                if (fighterRequirementsResult == false)
                {
                    Debug.LogError($"Error loading fighter {Name}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }


            // Load movesets.
            try
            {
                movesets = new Moveset[movesetReferences.Length];
                for (int i = 0; i < movesetReferences.Length; i++)
                {
                    var movesetLoadResult = await AddressablesManager.LoadAssetAsync(movesetReferences[i]);
                    movesets[i] = movesetLoadResult;
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
            return fighter;
        }

        public override string GetFighterGUID()
        {
            return fighterReference.AssetGUID;
        }

        
        public override Moveset[] GetMovesets()
        {
            return movesets;
        }

        public override bool Unload()
        {
            fighter = null;
            movesets = null;
            for (int i = 0; i < movesetReferences.Length; i++)
            {
                AddressablesManager.ReleaseAsset(movesetReferences[i]);
            }
            AddressablesManager.ReleaseAsset(fighterReference);
            return true;
        }
    }
}