using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

// https://gist.github.com/MPozek/f13eea941a7d59b7d4bdf0f83a2e4534
/*[CustomPropertyDrawer(typeof(Enum), true)]
public class EnumPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position = EditorGUI.PrefixLabel(position, label);

        if (GUI.Button(
                position,
                new GUIContent(property.enumDisplayNames[property.enumValueIndex], property.enumDisplayNames[property.enumValueIndex]), 
                EditorStyles.popup
            ))
        {
            AdvancedStringOptionsDropdown _dropdown = new AdvancedStringOptionsDropdown(property.enumDisplayNames);
            _dropdown.selectedProperty = property;
            _dropdown.Show(position);
        }
    }
}*/

public class AdvancedStringOptionsDropdown : AdvancedDropdown
{
    public SerializedProperty selectedProperty;
    private string[] _enumNames;

    public AdvancedStringOptionsDropdown(string[] stringOptions) : base(new AdvancedDropdownState())
    {
        _enumNames = stringOptions;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        selectedProperty.enumValueIndex = item.id;
        selectedProperty.serializedObject.ApplyModifiedProperties();
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("");

        for (int i = 0; i < _enumNames.Length; i++)
        {
            var item = new AdvancedDropdownItem(_enumNames[i]);
            item.id = i;

            root.AddChild(item);
        }

        return root;
    }
}