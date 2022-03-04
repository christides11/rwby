using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace rwby
{
    public class AttackDefinitionEditorWindow : HnSF.Combat.AttackDefinitionEditorWindow
    {
        public static void Init(AttackDefinition attack)
        {
            AttackDefinitionEditorWindow window = ScriptableObject.CreateInstance<AttackDefinitionEditorWindow>();
            window.attack = attack;
            window.Show();
        }

        protected override void Update()
        {
            base.Update();
        }


        [SerializeField] AttackDefinitionPreviewScene previewScene;
        string fighterString;
        //IFighterDefinition fDefinition;
        protected override void DrawGeneralOptions(SerializedObject serializedObject)
        {
            base.DrawGeneralOptions(serializedObject);
            fighterString = EditorGUILayout.TextField("Fighter", fighterString);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Viewer"))
            {
                _ = SetupViewerScene();
            }
            if (GUILayout.Button("Close Viewer"))
            {
                CleanupViewerScene();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected virtual async UniTaskVoid SetupViewerScene()
        {
            /*
            previewScene = AttackDefinitionPreviewScene.ShowWindow(null);
            await previewScene.manag.GetComponentInChildren<ModLoader>().Initialize();
            await ContentManager.singleton.LoadContentDefinition<IFighterDefinition>(new ModObjectReference(fighterString));

            IFighterDefinition fd = ContentManager.singleton.GetContentDefinition<IFighterDefinition>(new ModObjectReference(fighterString));
            await fd.Load();

            visualFighterSceneReference = GameObject.Instantiate(fd.GetFighter());
            EditorSceneManager.MoveGameObjectToScene(visualFighterSceneReference, previewScene.sceneLoaded);
            visualFighterSceneReference.transform.position = Vector3.up;
            visualFighterPrefab = fd.GetFighter();

            Selection.activeObject = visualFighterSceneReference;
            previewScene.FrameSelected();*/

            //visualFighterSceneReference.GetComponent<FighterManager>().Awake();
            //visualFighterSceneReference.GetComponent<FighterManager>().Spawned();
        }

        protected virtual void CleanupViewerScene()
        {
            /*
            previewScene.manag.GetComponentInChildren<ModLoader>().UnloadAllMods();
            ContentManager.singleton.UnloadContentDefinition<IFighterDefinition>(new ModObjectReference(fighterString));
            ContentManager.singleton = null;
            previewScene.Close();*/
        }

        protected virtual void SetupFighter()
        {
            /*
            FighterManager fman = visualFighterSceneReference.GetComponent<FighterManager>();
            fman.Awake();*/
        }

        /*
        protected override void ResetFighterVariables()
        {
            //visualFighterSceneReference.transform.position = new Vector3(0, 1, 0);
        }*/

        protected override void IncrementForward()
        {
            timelineFrame = Mathf.Min(timelineFrame + 1, attack.length);

            // TODO: Increment Fighter
        }
    }
}