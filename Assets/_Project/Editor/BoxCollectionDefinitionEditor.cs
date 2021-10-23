using HnSF.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomEditor(typeof(BoxCollectionDefinition), true)]
    public class BoxCollectionDefinitionEditor : HnSF.Combat.BoxCollectionDefinitionEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();
        }

        protected bool collboxGroupsDropdown;
        protected bool throwableboxGroupsDropdown;
        public override void CreateMenus()
        {
            base.CreateMenus();

            GenericMenu collboxMenu = new GenericMenu();
            foreach (string hType in hurtboxGroupTypes.Keys)
            {
                string destination = hType.Replace('.', '/');
                collboxMenu.AddItem(new GUIContent(destination), true, OnCollboxGroupTypeSelected, hType);
            }
            DrawBoxSection(collboxMenu, serializedObject, "Add Collisionbox Group", "collboxGroups", ref collboxGroupsDropdown);

            GenericMenu throwableboxMenu = new GenericMenu();
            foreach (string hType in hurtboxGroupTypes.Keys)
            {
                string destination = hType.Replace('.', '/');
                throwableboxMenu.AddItem(new GUIContent(destination), true, OnThrowableboxGroupTypeSelected, hType);
            }
            DrawBoxSection(throwableboxMenu, serializedObject, "Add Throwablebox Group", "throwableboxGroups", ref throwableboxGroupsDropdown);
        }

        protected virtual void OnCollboxGroupTypeSelected(object type)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty("collboxGroups");
            property.InsertArrayElementAtIndex(property.arraySize);
            property.GetArrayElementAtIndex(property.arraySize - 1).managedReferenceValue = Activator.CreateInstance(hurtboxGroupTypes[(string)type]);
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnThrowableboxGroupTypeSelected(object type)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty("throwableboxGroups");
            property.InsertArrayElementAtIndex(property.arraySize);
            property.GetArrayElementAtIndex(property.arraySize - 1).managedReferenceValue = Activator.CreateInstance(hurtboxGroupTypes[(string)type]);
            serializedObject.ApplyModifiedProperties();
        }
    }
}