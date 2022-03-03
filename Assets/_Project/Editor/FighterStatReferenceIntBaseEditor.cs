using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace rwby
{
    /*
    [CustomPropertyDrawer(typeof(FighterBaseStatReferenceInt), true)]
    public class FighterStatReferenceIntBaseEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float defaultSize = EditorGUIUtility.singleLineHeight * 4 + 14;

            return defaultSize;
        }
        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = 18;
            
            float yPosition = position.y;

            EditorGUI.PropertyField(new Rect(position.x, yPosition, 100, lineHeight),
                property.FindPropertyRelative("variable"), new GUIContent("Tehe"));
            
            EditorGUI.EndProperty();
        }
    }*/
}