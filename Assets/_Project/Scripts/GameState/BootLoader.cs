using Cysharp.Threading.Tasks;
using rwby.Debugging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class BootLoader : MonoBehaviour
    {
        /*
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Settings gameSettings;

        [SerializeField] private ConsoleReader consoleReader;
        [SerializeField] private ConsoleWindow consoleWindow;*/

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

            /*
            await UniTask.WaitForEndOfFrame();
            if (useArgs && Application.isEditor)
            {
                foreach (string s in args)
                {
                    _ = consoleReader.Convert(s);
                }
            }*/
        }
    }
}