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
        
        async UniTask Awake()
        {
            if (bootLoaded) return;
            bootLoaded = true;
            //DontDestroyOnLoad(gameObject);
            //singleton = this;
            managersObject = GameObject.Instantiate(managersPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(managersObject);
            await managersObject.GetComponentInChildren<GameManager>().Initialize();
            GameManager gameManager = managersObject.GetComponentInChildren<GameManager>();

            var guidRef = new ModContentGUIDReference()
            {
                contentGUID = menuSoundseference.contentGUID,
                contentType = (int)ContentType.Soundbank,
                modGUID = menuSoundseference.modGUID
            };
            var rawRef = ContentManager.singleton.ConvertModContentGUIDReference(guidRef);
            
            await gameManager.contentManager.LoadContentDefinition(rawRef);

            await UniTask.WaitForEndOfFrame();
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