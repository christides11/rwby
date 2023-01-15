using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using IngameDebugConsole;
using UnityEngine;

namespace rwby
{
    public class BootLoader : MonoBehaviour
    {
        public static bool bootLoaded = false;

        [SerializeField] private GameObject managersPrefab;
        private GameObject managersObject;

        public bool useArgs = false;
        public List<string> args = new List<string>();

        public ModObjectSetContentReference menuSoundseference;

        public int delayFrames = 5;
        
        async UniTask Awake()
        {
            if (bootLoaded) return;
            bootLoaded = true;
            managersObject = GameObject.Instantiate(managersPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(managersObject);
            await managersObject.GetComponentInChildren<GameManager>().Initialize();
            GameManager gameManager = managersObject.GetComponentInChildren<GameManager>();

            var guidRef = new ModContentStringReference()
            {
                contentGUID = menuSoundseference.contentGUID,
                contentType = ContentType.Soundbank,
                modGUID = menuSoundseference.modGUID
            };
            var rawRef = ContentManager.singleton.ConvertStringToGUIDReference(guidRef);
            
            await gameManager.contentManager.LoadContentDefinition(rawRef);

            for (int i = 0; i < delayFrames; i++)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            
            if (useArgs && Application.isEditor)
            {
                foreach (string s in args)
                {
                    DebugLogConsole.ExecuteCommand(s);
                }
            }
        }
    }
}