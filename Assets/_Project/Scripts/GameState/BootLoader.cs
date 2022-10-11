using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using IngameDebugConsole;
using UnityEngine;

namespace rwby
{
    public class BootLoader : MonoBehaviour
    {
        public static BootLoader singleton;

        [SerializeField] private GameObject managersPrefab;
        private GameObject managersObject;

        public bool useArgs = false;
        public List<string> args = new List<string>();

        async UniTask Awake()
        {
            if(singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            singleton = this;
            managersObject = GameObject.Instantiate(managersPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(managersObject);
            await managersObject.GetComponentInChildren<GameManager>().Initialize();
            GameManager gameManager = managersObject.GetComponentInChildren<GameManager>();
            
            
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