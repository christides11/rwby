using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using rwby;

namespace rwby
{
    public class ContentManager : MonoBehaviour
    {
        public static ContentManager instance;

        [SerializeField] private ModLoader modLoader;

        public void Initialize()
        {
            instance = this;
        }

        #region General
        public bool ContentExist(ContentType contentType, ModObjectReference objectReference)
        {
            if (!modLoader.loadedMods.ContainsKey(objectReference.modIdentifier))
            {
                return false;
            }
            return modLoader.loadedMods[objectReference.modIdentifier].definition.ContentExist(contentType, objectReference.objectIdentifier);
        }

        public async UniTask<bool> LoadContentDefinitions(ContentType contentType)
        {
            bool result = true;
            foreach (string m in modLoader.loadedMods.Keys)
            {
                bool r = await LoadContentDefinitions(contentType, m);
                if (r == false)
                {
                    result = false;
                }
            }
            return result;
        }

        public async UniTask<bool> LoadContentDefinitions(ContentType contentType, string modIdentifier)
        {
            if (!modLoader.loadedMods.ContainsKey(modIdentifier))
            {
                return false;
            }
            return await modLoader.loadedMods[modIdentifier].definition.LoadContentDefinitions(contentType);
        }

        public async UniTask<bool> LoadContentDefinition(ContentType contentType, ModObjectReference objectReference)
        {
            if (!modLoader.loadedMods.ContainsKey(objectReference.modIdentifier))
            {
                Debug.LogError($"Error loading content {objectReference}: mod {objectReference.modIdentifier} not found.");
                return false;
            }
            return await modLoader.loadedMods[objectReference.modIdentifier].definition.LoadContentDefinition(contentType, objectReference.objectIdentifier);
        }

        public List<ModObjectReference> GetContentDefinitionReferences(ContentType contentType)
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            foreach (string m in modLoader.loadedMods.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences(contentType, m));
            }
            return content;
        }

        public List<ModObjectReference> GetContentDefinitionReferences(ContentType contentType, string modIdentifier)
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            // Mod does not exist.
            if (!modLoader.loadedMods.ContainsKey(modIdentifier))
            {
                return content;
            }
            List<IContentDefinition> fds = modLoader.loadedMods[modIdentifier].definition.GetContentDefinitions(contentType);
            if (fds == null)
            {
                return content;
            }
            foreach (IContentDefinition fd in fds)
            {
                content.Add(new ModObjectReference(modIdentifier, fd.Identifier));
            }
            return content;
        }

        public IContentDefinition GetContentDefinition(ContentType contentType, ModObjectReference reference)
        {
            if (!modLoader.loadedMods.ContainsKey(reference.modIdentifier))
            {
                return null;
            }

            IContentDefinition g = modLoader.loadedMods[reference.modIdentifier].definition.GetContentDefinition(contentType, reference.objectIdentifier);
            return g;
        }

        public void UnloadContentDefinitions(ContentType contentType)
        {
            foreach (string m in modLoader.loadedMods.Keys)
            {
                UnloadContentDefinitions(contentType, m);
            }
        }

        public void UnloadContentDefinitions(ContentType contentType, string modIdentifier)
        {
            if (!modLoader.loadedMods.ContainsKey(modIdentifier))
            {
                return;
            }
            modLoader.loadedMods[modIdentifier].definition.UnloadContentDefinitions(contentType);
        }
        #endregion

        #region Stages
        public async UniTask<bool> LoadMap(ModObjectReference map)
        {
            if (!modLoader.loadedMods.TryGetValue(map.modIdentifier, out LoadedModDefinition mod))
            {
                return false;
            }

            IMapDefinition sd = (IMapDefinition)mod.definition.GetContentDefinition(ContentType.Map, map.objectIdentifier);
            if (sd == null)
            {
                return false;
            }

            await sd.LoadMap(UnityEngine.SceneManagement.LoadSceneMode.Additive);
            return true;
        }
        #endregion
    }
}