using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomEditor(typeof(UModModDefinition), true)]
    public class UMOdModDefinitionEditor : Editor
    {
        protected Dictionary<string, (string, Type)> hurtboxGroupTypes = new Dictionary<string, (string, Type)>();

        public virtual void OnEnable()
        {
            hurtboxGroupTypes.Clear();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var givenType in a.GetTypes())
                {
                    if (givenType.IsSubclassOf(typeof(IContentParser))
                        && givenType.ContainsGenericParameters == false)
                    {
                        UModContentParserAttribute acp = (UModContentParserAttribute)givenType.GetCustomAttribute(typeof(UModContentParserAttribute), true);
                        if (acp == null) continue;

                        hurtboxGroupTypes.Add(acp.parsetPath, (acp.parserNickname, givenType));
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            /*
            EditorGUILayout.PropertyField(serializedObject.FindProperty("compatibilityLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("versionStrictness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));*/
            
            if (GUILayout.Button("Add Content Parser"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (string hType in hurtboxGroupTypes.Keys)
                {
                    string destination = hType.Replace('.', '/');
                    menu.AddItem(new GUIContent(destination), true, OnHurtboxGroupTypeSelected, hType);
                }
                menu.ShowAsContext();
            }
            
            /*
            EditorGUILayout.LabelField("Content Parsers");
            SerializedProperty property = serializedObject.FindProperty("contentParsers");

            for (int i = 0; i < property.arraySize; i++)
            {
                var nameProperty = property.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                EditorGUILayout.LabelField(nameProperty != null ? nameProperty.stringValue : "Parser");
                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
            }*/

            serializedObject.ApplyModifiedProperties();
        }

        private void OnHurtboxGroupTypeSelected(object ty)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty("contentParsers");

            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).GetType() == hurtboxGroupTypes[(string)ty].Item2)
                {
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }

            property.InsertArrayElementAtIndex(property.arraySize);
            property.GetArrayElementAtIndex(property.arraySize - 1).managedReferenceValue = Activator.CreateInstance(hurtboxGroupTypes[(string)ty].Item2);
            property.GetArrayElementAtIndex(property.arraySize - 1).FindPropertyRelative("name").stringValue = hurtboxGroupTypes[(string)ty].Item1;
            serializedObject.ApplyModifiedProperties();
        }
    }
}