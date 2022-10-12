using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace rwby
{
    public class RwbyModToolsWindow : EditorWindow
    {
        public int currentTab = 0;

        public static ListRequest listRequest;

        // Add menu named "My Window" to the Window menu
        [MenuItem("RWBY/Initializer")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            RwbyModToolsWindow window = (RwbyModToolsWindow)EditorWindow.GetWindow(typeof(RwbyModToolsWindow));
            window.Show();
        }

        private void OnEnable()
        {
            listRequest = Client.List();
        }

        private void OnGUI()
        {
            switch (currentTab)
            {
                case 0:
                    InitializationTab();
                    break;
            }
        }

        private void InitializationTab()
        {
            if (listRequest.Status != StatusCode.Success)
            {
                return;
            }

            int packagesInstalled = 0;
            GUILayout.Label("1. Install Packages");
            for (int i = 0; i < RwbyDependencies.packages.Length; i++)
            {
                var pName = RwbyDependencies.packages[i];
                var res = listRequest.Result.FirstOrDefault(x => ($"{x.name}@{x.version}" == pName.Item1));
                if (res == null)
                {
                    if (GUILayout.Button($"{pName.Item1}"))
                    {
                        AddRequest currentRequest = Client.Add(RwbyDependencies.packages[i].Item2);
                    }
                }
                else
                {
                    packagesInstalled++;
                }
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(packagesInstalled < RwbyDependencies.packages.Length);
            GUILayout.Label("2. Finalize");
            if (GUILayout.Button("Finalize"))
            {
                // GET DLLs
                string path = EditorUtility.OpenFilePanel("Select Game Executable", "", "exe");

                if (!File.Exists(path))
                {
                    Debug.LogError("Could not find game executable.");
                    return;
                }

                var source = Path.GetDirectoryName(path) + @"\rwby_Data\Managed";

                var destination = Application.dataPath + @"/assemblies";
                if (Directory.Exists(destination))
                {
                    Directory.Delete(destination, true);
                }
                Directory.CreateDirectory(destination);

                foreach (string assembly in RwbyDependencies.dlls)
                {
                    if (File.Exists($@"{source}\{assembly}.dll"))
                    {
                        File.Copy($@"{source}\{assembly}.dll", $@"{destination}/{assembly}.dll", true);
                    }
                    else
                    {
                        Debug.LogWarning($@"Modding Tools: Couldn't find {assembly}.dll. Skipping.");
                    }
                }

                // IMPORT PACKAGE
                AssetDatabase.ImportPackage(Path.GetDirectoryName(path) + @"\Modding\rwby-mod-tool.unitypackage", false);

                AssetDatabase.Refresh();

                ClearConsole();

                Debug.Log("Successfully imported mod tools.");
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}