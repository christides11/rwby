using System;
using System.Text;
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
            string tempString = BuildString(maxLength, property.FindPropertyRelative("guid"));
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
        
        public static string BuildString(byte length, SerializedProperty sp)
        {
            try
            {
                StringBuilder sb = new StringBuilder("", length);

                for (int i = 0; i < sp.arraySize; i++)
                {
                    byte value = (byte)sp.GetArrayElementAtIndex(i).intValue;
                    if (value == 0) break;
                    if (value >= ContentGUID.byteToLetterLookup.Length) throw new Exception($"GUID byte {value} out of range.");
                    sb.Append(ContentGUID.byteToLetterLookup[value - 1]);
                }

                return sb.ToString();
            }
            catch(Exception e)
            {
                Debug.LogError($"Error building string: {e}");
                return "";
            }
        }
    }
}