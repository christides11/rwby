using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomEditor(typeof(AttackDefinition), true)]
    public class AttackDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor", GUILayout.Width(Screen.width), GUILayout.Height(45)))
            {
                rwby.AttackDefinitionEditorWindow.Init(target as rwby.AttackDefinition);
            }

            DrawDefaultInspector();
        }
    }
}