using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomPropertyDrawer(typeof(ContentGUID))]
    public class ContentGUIDPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            byte maxLength = (byte)property.FindPropertyRelative("guid").arraySize;
            string tempString = ContentGUID.BuildString(maxLength, property.FindPropertyRelative("guid"));
            string oldString = tempString;

            tempString = EditorGUI.TextField(position, label, tempString);

            if (tempString == oldString)
            {
                EditorGUI.EndProperty();
                return;
            }

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
            EditorGUI.EndProperty();
        }
    }
}