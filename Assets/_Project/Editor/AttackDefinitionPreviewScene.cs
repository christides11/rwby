using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace rwby
{
    public class AttackDefinitionPreviewScene : SceneView
    {
        public GameObject managersPrefab;
        private static Scene scene;

        public Scene sceneLoaded;

        public static AttackDefinitionPreviewScene ShowWindow(GameObject stageObj)
        {
            // Create the window
            AttackDefinitionPreviewScene window = CreateWindow<AttackDefinitionPreviewScene>("Scene");

            window.titleContent = new GUIContent("AAA");
            window.titleContent.image = EditorGUIUtility.IconContent("GameObject Icon").image;

            scene = EditorSceneManager.NewPreviewScene();
            
            window.sceneLoaded = scene;
            window.sceneLoaded.name = window.name;
            window.customScene = window.sceneLoaded;

            window.drawGizmos = false;

            window.SetupScene();

            window.Repaint();

            return window;
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
        }

        public GameObject manag;
        void SetupScene()
        {
            // Create light
            GameObject lightingObj = new GameObject("Lighting");
            lightingObj.transform.eulerAngles = new Vector3(50, -30, 0);
            lightingObj.AddComponent<Light>().type = UnityEngine.LightType.Directional;

            // Create floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.localScale = new Vector3(100, 1, 100);
            floor.transform.position = new Vector3(0, 0, 0);

            // Create manager.
            manag = GameObject.Instantiate(managersPrefab);


            EditorSceneManager.MoveGameObjectToScene(lightingObj, scene);
            EditorSceneManager.MoveGameObjectToScene(floor, scene);
            EditorSceneManager.MoveGameObjectToScene(manag, scene);
            ContentManager.singleton = manag.GetComponentInChildren<ContentManager>();
        }

        new void OnGUI()
        {
            base.OnGUI();
        }
    }
}