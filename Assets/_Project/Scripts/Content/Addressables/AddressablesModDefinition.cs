using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesModDefinition", menuName = "rwby/Content/Addressables/ModDefinition")]
    public class AddressablesModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class IdentifierAssetReferenceRelation<T> where T : UnityEngine.Object
        {
            public string identifier;
            public AssetReferenceT<T> asset;
        }

        public string Description { get { return description; } }

        [TextArea] [SerializeField] private string description;

        [SerializeField] private List<IdentifierAssetReferenceRelation<IFighterDefinition>> fighterRefs = new List<IdentifierAssetReferenceRelation<IFighterDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IGameModeDefinition>> gamemodeRefs = new List<IdentifierAssetReferenceRelation<IGameModeDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IGameModeComponentDefinition>> gamemodeComponentRefs = new List<IdentifierAssetReferenceRelation<IGameModeComponentDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IMapDefinition>> mapRefs = new List<IdentifierAssetReferenceRelation<IMapDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<ISoundbankDefinition>> soundbankRefs = new List<IdentifierAssetReferenceRelation<ISoundbankDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IEffectbankDefinition>> effectbankRefs = new List<IdentifierAssetReferenceRelation<IEffectbankDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IFighterDefinition>> fighterReferences = new Dictionary<string, AssetReferenceT<IFighterDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IFighterDefinition>> fighterDefinitions
            = new Dictionary<string, OperationResult<IFighterDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IGameModeDefinition>> gamemodeReferences = new Dictionary<string, AssetReferenceT<IGameModeDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IGameModeDefinition>> gamemodeDefinitions
            = new Dictionary<string, OperationResult<IGameModeDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IGameModeComponentDefinition>> gamemodeComponentReferences = new Dictionary<string, AssetReferenceT<IGameModeComponentDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IGameModeComponentDefinition>> gamemodeComponentDefinitions
            = new Dictionary<string, OperationResult<IGameModeComponentDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IMapDefinition>> mapReferences = new Dictionary<string, AssetReferenceT<IMapDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IMapDefinition>> mapDefinitions
            = new Dictionary<string, OperationResult<IMapDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<ISoundbankDefinition>> soundbankReferences = new Dictionary<string, AssetReferenceT<ISoundbankDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<ISoundbankDefinition>> soundbankDefinitions
            = new Dictionary<string, OperationResult<ISoundbankDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IEffectbankDefinition>> effectbankReferences = new Dictionary<string, AssetReferenceT<IEffectbankDefinition>>();
        [NonSerialized]
        private Dictionary<string, OperationResult<IEffectbankDefinition>> effectbankDefinitions
            = new Dictionary<string, OperationResult<IEffectbankDefinition>>();

        public void OnEnable()
        {
            fighterReferences.Clear();
            gamemodeReferences.Clear();
            gamemodeComponentReferences.Clear();
            mapReferences.Clear();
            soundbankReferences.Clear();
            effectbankReferences.Clear();
            foreach (IdentifierAssetReferenceRelation<IFighterDefinition> a in fighterRefs)
            {
                fighterReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IGameModeDefinition> a in gamemodeRefs)
            {
                gamemodeReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IGameModeComponentDefinition> a in gamemodeComponentRefs)
            {
                gamemodeComponentReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IMapDefinition> a in mapRefs)
            {
                mapReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<ISoundbankDefinition> a in soundbankRefs)
            {
                soundbankReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IEffectbankDefinition> a in effectbankRefs)
            {
                effectbankReferences.Add(a.identifier, a.asset);
            }
        }

        #region Content
        public bool ContentExist(ContentType contentType, string contentIdentfier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return fighterReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Gamemode:
                    return gamemodeReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.GamemodeComponent:
                    return gamemodeComponentReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Map:
                    return mapReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Soundbank:
                    return soundbankReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Effectbank:
                    return effectbankReferences.ContainsKey(contentIdentfier) ? true : false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async UniTask<bool> LoadContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinitions(fighterReferences, fighterDefinitions);
                case ContentType.Gamemode:
                    return await LoadContentDefinitions(gamemodeReferences, gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinitions(gamemodeComponentReferences, gamemodeComponentDefinitions);
                case ContentType.Map:
                    return await LoadContentDefinitions(mapReferences, mapDefinitions);
                case ContentType.Soundbank:
                    return await LoadContentDefinitions(soundbankReferences, soundbankDefinitions);
                case ContentType.Effectbank:
                    return await LoadContentDefinitions(effectbankReferences, effectbankDefinitions);
            }
            return false;
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIdentifier"></param>
        /// <returns></returns>
        public async UniTask<bool> LoadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinition(fighterReferences, fighterDefinitions, contentIdentifier);
                case ContentType.Gamemode:
                    return await LoadContentDefinition(gamemodeReferences, gamemodeDefinitions, contentIdentifier);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinition(gamemodeComponentReferences, gamemodeComponentDefinitions, contentIdentifier);
                case ContentType.Map:
                    return await LoadContentDefinition(mapReferences, mapDefinitions, contentIdentifier);
                case ContentType.Soundbank:
                    return await LoadContentDefinition(soundbankReferences, soundbankDefinitions, contentIdentifier);
                case ContentType.Effectbank:
                    return await LoadContentDefinition(effectbankReferences, effectbankDefinitions, contentIdentifier);
                default:
                    return false;
            }
        }

        public List<IContentDefinition> GetContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return GetContentDefinitions(fighterReferences, fighterDefinitions);
                case ContentType.Gamemode:
                    return GetContentDefinitions(gamemodeReferences, gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return GetContentDefinitions(gamemodeComponentReferences, gamemodeComponentDefinitions);
                case ContentType.Map:
                    return GetContentDefinitions(mapReferences, mapDefinitions);
                case ContentType.Soundbank:
                    return GetContentDefinitions(soundbankReferences, soundbankDefinitions);
                case ContentType.Effectbank:
                    return GetContentDefinitions(effectbankReferences, effectbankDefinitions);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIdentifier"></param>
        /// <returns></returns>
        public IContentDefinition GetContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return GetContentDefinition(fighterDefinitions, contentIdentifier);
                case ContentType.Gamemode:
                    return GetContentDefinition(gamemodeDefinitions, contentIdentifier);
                case ContentType.GamemodeComponent:
                    return GetContentDefinition(gamemodeComponentDefinitions, contentIdentifier);
                case ContentType.Map:
                    return GetContentDefinition(mapDefinitions, contentIdentifier);
                case ContentType.Soundbank:
                    return GetContentDefinition(soundbankDefinitions, contentIdentifier);
                case ContentType.Effectbank:
                    return GetContentDefinition(effectbankDefinitions, contentIdentifier);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        public void UnloadContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    UnloadContentDefinitions(fighterReferences, fighterDefinitions);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinitions(gamemodeReferences, gamemodeDefinitions);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinitions(gamemodeComponentReferences, gamemodeComponentDefinitions);
                    break;
                case ContentType.Map:
                    UnloadContentDefinitions(mapReferences, mapDefinitions);
                    break;
                case ContentType.Soundbank:
                    UnloadContentDefinitions(soundbankReferences, soundbankDefinitions);
                    break;
                case ContentType.Effectbank:
                    UnloadContentDefinitions(effectbankReferences, effectbankDefinitions);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIdentifier"></param>
        public void UnloadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    UnloadContentDefinition(fighterReferences, fighterDefinitions, contentIdentifier);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinition(gamemodeReferences, gamemodeDefinitions, contentIdentifier);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinition(gamemodeComponentReferences, gamemodeComponentDefinitions, contentIdentifier);
                    break;
                case ContentType.Map:
                    UnloadContentDefinition(mapReferences, mapDefinitions, contentIdentifier);
                    break;
                case ContentType.Soundbank:
                    UnloadContentDefinition(soundbankReferences, soundbankDefinitions, contentIdentifier);
                    break;
                case ContentType.Effectbank:
                    UnloadContentDefinition(effectbankReferences, effectbankDefinitions, contentIdentifier);
                    break;
            }
        }
        #endregion

        #region Shared
        protected async UniTask<bool> LoadContentDefinitions<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions) where T : IContentDefinition
        {
            // All of the content is already loaded.
            if (definitions.Count == references.Count)
            {
                return true;
            }
            try
            {
                foreach (string contentIdentifier in references.Keys)
                {
                    await LoadContentDefinition(references, definitions, contentIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return false;
        }

        protected async UniTask<bool> LoadContentDefinition<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Content doesn't exist.
            if (references.ContainsKey(contentIdentifier) == false)
            {
                Debug.LogError($"Error loading {contentIdentifier}: Content does not exist.");
                return false;
            }
            // Content already loaded.
            if (definitions.ContainsKey(contentIdentifier) == true)
            {
                if (definitions[contentIdentifier].Succeeded == true)
                {
                    return true;
                }
                Debug.LogError($"Error loading {contentIdentifier}: content is currently in progress of loading.");
                return false;
            }
            definitions.Add(contentIdentifier, new OperationResult<T>());
            OperationResult<T> result = await AddressablesManager.LoadAssetAsync<T>(references[contentIdentifier]);
            if (result.Succeeded)
            {
                result.Value.Identifier = contentIdentifier;
                definitions[contentIdentifier] = result;
                return true;
            }
            Debug.LogError($"Error loading {contentIdentifier}: could not load.");
            definitions.Remove(contentIdentifier);
            return false;
        }

        protected IContentDefinition GetContentDefinition<T>(Dictionary<string, OperationResult<T>> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Content does not exist, or was not loaded.
            if (definitions.ContainsKey(contentIdentifier) == false)
            {
                return null;
            }
            return definitions[contentIdentifier].Value;
        }

        protected List<IContentDefinition> GetContentDefinitions<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions) where T : IContentDefinition
        {
            List<IContentDefinition> contentList = new List<IContentDefinition>();
            foreach (var content in definitions.Values)
            {
                contentList.Add(content.Value);
            }
            return contentList;
        }

        protected void UnloadContentDefinitions<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions) where T : IContentDefinition
        {
            foreach (var v in definitions)
            {
                AddressablesManager.ReleaseAsset(references[v.Key]);
            }
            definitions.Clear();
        }

        protected void UnloadContentDefinition<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Fighter is not loaded.
            if (fighterDefinitions.ContainsKey(contentIdentifier) == false)
            {
                return;
            }
            AddressablesManager.ReleaseAsset(fighterReferences[contentIdentifier]);
            fighterDefinitions.Remove(contentIdentifier);
        }
        #endregion
    }
}