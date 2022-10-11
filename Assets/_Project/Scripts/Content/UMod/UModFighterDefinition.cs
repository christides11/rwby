using System;
using Cysharp.Threading.Tasks;
using UMod;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModFighterDefinition", menuName = "rwby/Content/UMod/FighterDefinition")]
    public class UModFighterDefinition : IFighterDefinition, IUModModHostRef
    {
        public ModHost modHost { get; set; } = null;
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

        public override ModObjectItemReference[] huds
        {
            get { return hudContentReferences; }
        }
        
        public override CameraDef[] cameras => cams;

        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private ModObjectItemReference[] hudContentReferences = Array.Empty<ModObjectItemReference>();
        [SerializeField] private CameraDef[] cams = Array.Empty<CameraDef>();
        [SerializeField] private bool selectable = true;
        [SerializeField] private int health;
        [SerializeField] private int aura;
        [SerializeField] private int auraGainPerFrame;

        [SerializeField] private string fighterReference = "";
        [SerializeField] private string[] movesetReferences = Array.Empty<string>();
        [SerializeField] private ModGameAssetReference testFighterReference;

        [NonSerialized] private ModAsyncOperation<Moveset>[] movesetHandles = null;
        [NonSerialized] private ModAsyncOperation<GameObject> fighterHandle = null;

        public override async UniTask<bool> Load()
        {
            bool fighterLoadResult = await TryLoadFighter();
            if (!fighterLoadResult) return false;
            movesetHandles = new ModAsyncOperation<Moveset>[movesetReferences.Length];
            for (int i = 0; i < movesetReferences.Length; i++)
            {
                bool movesetLoadResult = await TryLoadMoveset(i);
                if (!movesetLoadResult) return false;
            }
            return true;
        }

        private async UniTask<bool> TryLoadMoveset(int index)
        {
            if (movesetHandles[index] != null && movesetHandles[index].IsSuccessful) return true;

            if (movesetHandles[index] == null) movesetHandles[index] = modHost.Assets.LoadAsync<Moveset>(movesetReferences[index]);

            if (movesetHandles[index].IsDone == false || !movesetHandles[index].IsSuccessful) await movesetHandles[index];

            if (movesetHandles[index].IsSuccessful) return true;
                
            Debug.LogError($"Error loading fighter {Name}: Could not load moveset {index}.");
            return false;
        }

        private async UniTask<bool> TryLoadFighter()
        {
            if (fighterHandle != null && fighterHandle.IsSuccessful) return true;

            if (fighterHandle == null) fighterHandle = modHost.Assets.LoadAsync<GameObject>(fighterReference);

            if (fighterHandle.IsDone == false || !fighterHandle.IsSuccessful) await fighterHandle;

            if (fighterHandle.IsSuccessful)
            {
                bool fighterRequirementsResult = await fighterHandle.Result.GetComponent<FighterManager>().OnFighterLoaded();
                if (fighterRequirementsResult == false)
                {
                    Debug.LogError($"Error loading fighter {Name}: Requirements could not be met.");
                    return false;
                }
                return true;
            }
            Debug.LogError($"Error loading fighter {Name}: Could not load from reference {fighterReference}.");
            return false;
        }

        public override GameObject GetFighter()
        {
            return fighterHandle.Result;
        }

        public override string GetFighterGUID()
        {
            throw new System.NotImplementedException();
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
                if(t.IsSuccessful) t.Reset();
            }
            movesetHandles = null;
            if(fighterHandle.IsSuccessful) fighterHandle.Reset();
            fighterHandle = null;
            return true;
        }
    }
}