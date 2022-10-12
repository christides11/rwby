using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomPropertyDrawer(typeof(UModAssetReference), true)]
    public class UModAssetReferenceEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = 18;
            
            float yPosition = position.y;
            
            EditorGUI.BeginProperty(position, label, property);
            
            EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width/2.0f, lineHeight), property.FindPropertyRelative("lookupTable"), GUIContent.none);

            if (property.FindPropertyRelative("lookupTable").objectReferenceValue != null)
            {
                UModAssetLookupTable lookupTable = (UModAssetLookupTable)property.FindPropertyRelative("lookupTable").objectReferenceValue;
                var tableArray = lookupTable.table.Values.ToArray();
                var tableID = property.FindPropertyRelative("tableID").intValue;
                if (!lookupTable.table.ContainsKey(tableID))
                {
                    EditorGUI.LabelField(new Rect(position.x+position.width/2.0f, yPosition, position.width/2.0f, lineHeight), tableID.ToString());
                    EditorGUI.EndProperty();
                    return;
                }
                var tableIDToArrayInt = Array.IndexOf(tableArray, lookupTable.table[tableID]);
                if (tableIDToArrayInt == -1)
                {
                    EditorGUI.LabelField(new Rect(position.x+position.width/2.0f, yPosition, position.width/2.0f, lineHeight), tableID.ToString());
                    EditorGUI.EndProperty();
                    return;
                }

                var value = EditorGUI.Popup(new Rect(position.x+position.width/2.0f, yPosition, position.width/2.0f, lineHeight), 
                    "", tableIDToArrayInt, tableArray);

                if (value != tableIDToArrayInt)
                {
                    property.FindPropertyRelative("tableID").intValue =
                        lookupTable.table.FirstOrDefault(x => x.Value == tableArray[value]).Key;
                }
            } 
            
            EditorGUI.EndProperty();
        }
    }
}