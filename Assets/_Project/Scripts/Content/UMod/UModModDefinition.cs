using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UMod;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModModDefinition", menuName = "Mahou/Content/UMod/ModDefinition")]
    public class UModModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class AssetReferenceRelation
        {
            public string identifier;
            public string assetPath;
        }

        public string Description { get { return description; } }

        [TextArea] [SerializeField] private string description;
        [SerializeField] private ModObjectSharedReference modNamespace;

        [SerializeField] private List<AssetReferenceRelation> fighters = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> gamemodes = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> gamemodeComponents = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> maps = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> battles = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> songs = new List<AssetReferenceRelation>();

        [NonSerialized] private Dictionary<string, string> fighterPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IFighterDefinition> fighterDefinitions = new Dictionary<string, IFighterDefinition>();

        [NonSerialized] private Dictionary<string, string> gamemodePaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IGameModeDefinition> gamemodeDefinitions = new Dictionary<string, IGameModeDefinition>();

        [NonSerialized] private Dictionary<string, string> gamemodeComponentPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IGameModeComponentDefinition> gamemodeComponentDefinitions = new Dictionary<string, IGameModeComponentDefinition>();

        [NonSerialized] private Dictionary<string, string> mapPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IMapDefinition> mapDefinitions = new Dictionary<string, IMapDefinition>();

        public void OnEnable()
        {
            fighterPaths.Clear();
            gamemodePaths.Clear();
            gamemodeComponents.Clear();
            mapPaths.Clear();
            foreach(AssetReferenceRelation r in fighters)
            {
                fighterPaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in gamemodes)
            {
                gamemodePaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in gamemodeComponents)
            {
                gamemodeComponentPaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in maps)
            {
                mapPaths.Add(r.identifier, r.assetPath);
            }
        }

        #region Content
        public bool ContentExist(ContentType contentType, string contentIdentfier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return fighterPaths.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Gamemode:
                    return gamemodePaths.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.GamemodeComponent:
                    return gamemodeComponentPaths.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Map:
                    return mapPaths.ContainsKey(contentIdentfier) ? true : false;
                default:
                    return false;
            }
        }

        public async UniTask<bool> LoadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            ModHost modHost = ModLoader.instance.loadedMods[modNamespace.reference.modIdentifier].host;
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinition(modHost, fighterPaths, fighterDefinitions, contentIdentifier);
                case ContentType.Gamemode:
                    return await LoadContentDefinition(modHost, gamemodePaths, gamemodeDefinitions, contentIdentifier);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinition(modHost, gamemodeComponentPaths, gamemodeComponentDefinitions, contentIdentifier);
                case ContentType.Map:
                    return await LoadContentDefinition(modHost, mapPaths, mapDefinitions, contentIdentifier);
                default:
                    return false;
            }
        }

        public async UniTask<bool> LoadContentDefinitions(ContentType contentType)
        {
            ModHost modHost = ModLoader.instance.loadedMods[modNamespace.reference.modIdentifier].host;
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinitions(modHost, fighterPaths, fighterDefinitions);
                case ContentType.Gamemode:
                    return await LoadContentDefinitions(modHost, gamemodePaths, gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinitions(modHost, gamemodeComponentPaths, gamemodeComponentDefinitions);
                case ContentType.Map:
                    return await LoadContentDefinitions(modHost, mapPaths, mapDefinitions);
                default:
                    return false;
            }
        }

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
                default:
                    return null;
            }
        }

        public List<IContentDefinition> GetContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return GetContentDefinitions(fighterDefinitions);
                case ContentType.Gamemode:
                    return GetContentDefinitions(gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return GetContentDefinitions(gamemodeComponentDefinitions);
                case ContentType.Map:
                    return GetContentDefinitions(mapDefinitions);
                default:
                    return null;
            }
        }

        public void UnloadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    UnloadContentDefinition(fighterDefinitions, contentIdentifier);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinition(gamemodeDefinitions, contentIdentifier);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinition(gamemodeComponentDefinitions, contentIdentifier);
                    break;
                case ContentType.Map:
                    UnloadContentDefinition(mapDefinitions, contentIdentifier);
                    break;
            }
        }

        public void UnloadContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    UnloadContentDefinitions(fighterDefinitions);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinitions(gamemodeDefinitions);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinitions(gamemodeComponentDefinitions);
                    break;
                case ContentType.Map:
                    UnloadContentDefinitions(mapDefinitions);
                    break;
            }
        }
#endregion

        #region Shared
        protected async UniTask<bool> LoadContentDefinitions<T>(ModHost modHost, Dictionary<string, string> paths,
            Dictionary<string, T> definitions) where T : IContentDefinition
        {
            // All of the content is already loaded.
            if (paths.Count == definitions.Count)
            {
                return true;
            }
            try
            {
                foreach (string contentIdentifier in paths.Keys)
                {
                    await LoadContentDefinition(modHost, paths, definitions, contentIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return false;
        }

        protected async UniTask<bool> LoadContentDefinition<T>(ModHost modHost, Dictionary<string, string> paths,
            Dictionary<string, T> definitions, string contentIdentifier) 
            where T : IContentDefinition
        {
            // Asset doesn't exist.
            if (paths.ContainsKey(contentIdentifier) == false)
            {
                //ConsoleWindow.current.WriteLine($"{contentIdentifier} does not exist for {modHost.name}.");
                return false;
            }
            // Content already loaded.
            if(definitions.ContainsKey(contentIdentifier) == true)
            {
                return true;
            }
            // Invalid path.
            if (modHost.Assets.Exists(paths[contentIdentifier]) == false)
            {
                //ConsoleWindow.current.WriteLine($"Content does not exist at path {paths[contentIdentifier]} for {modHost.name}.");
                return false;
            }
            ModAsyncOperation request = modHost.Assets.LoadAsync(paths[contentIdentifier]);
            await request;
            if (request.IsSuccessful)
            {
                (request.Result as T).Identifier = contentIdentifier;
                definitions.Add(contentIdentifier, request.Result as T);
                return true;
            }
            return false;
        }

        protected IContentDefinition GetContentDefinition<T>(Dictionary<string, T> definitions, string contentIdentifier)
            where T : IContentDefinition
        {
            // Content does not exist, or was not loaded.
            if (definitions.ContainsKey(contentIdentifier) == false)
            {
                return null;
            }
            return definitions[contentIdentifier];
        }

        protected List<IContentDefinition> GetContentDefinitions<T>(Dictionary<string, T> definitions) where T : IContentDefinition
        {
            List<IContentDefinition> contentList = new List<IContentDefinition>();
            foreach (var content in definitions.Values)
            {
                contentList.Add(content);
            }
            return contentList;
        }

        protected void UnloadContentDefinitions<T>(Dictionary<string, T> definitions) where T : IContentDefinition
        {
            definitions.Clear();
        }

        protected void UnloadContentDefinition<T>(Dictionary<string, T> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Fighter is not loaded.
            if (fighterDefinitions.ContainsKey(contentIdentifier) == false)
            {
                return;
            }
            fighterDefinitions.Remove(contentIdentifier);
        }
        #endregion
    }
}