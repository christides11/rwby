using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomPropertyDrawer(typeof(ContentGUID), true)]
    public class ContentGUIDPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            byte maxLength = (byte)property.FindPropertyRelative("guid").arraySize;
            string tempString = ContentGUID.BuildString(maxLength, property.FindPropertyRelative("guid"));

            tempString = EditorGUI.TextField(position, label, tempString);

            if (tempString.Length > maxLength)
                tempString = tempString.Remove(maxLength, tempString.Length-maxLength);

            if (ContentGUID.TryBuildGUID(maxLength, tempString, out byte[] output))
            {
                property.FindPropertyRelative("guid").ClearArray();
                for (int i = 0; i < output.Length; i++)
                {
                    property.FindPropertyRelative("guid").InsertArrayElementAtIndex(property.FindPropertyRelative("guid").arraySize);
                    property.FindPropertyRelative("guid").GetArrayElementAtIndex(i).intValue = output[i];
                }
            }
        }
    }
}