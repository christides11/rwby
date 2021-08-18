using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesMapDefinition", menuName = "rwby/Content/Addressables/MapDefinition")]
    public class AddressablesMapDefinition : IMapDefinition
    {
        public override string Name { get { return mapName; } }
        public override string SceneName { get { return sceneName; } }

        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string mapName;
        [SerializeField] private string sceneName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable;

        [SerializeField] private AssetReference sceneReference;

        public override async UniTask LoadMap(UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            await Addressables.LoadSceneAsync(sceneReference, loadMode);
        }

        public override async UniTask UnloadMap()
        {

        }
    }
}