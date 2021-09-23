using UnityEngine;
using UnityEditor;
using System;

namespace rwby
{
    [CustomPropertyDrawer(typeof(rwby.HitInfo), true)]
    public class HitInfoPropertyDrawer : HnSF.Combat.HitInfoPropertyDrawer
    {
        bool effectFoldoutGroup;
        protected override void DrawProperty(ref Rect position, SerializedProperty property, ref float yPosition)
        {
            base.DrawProperty(ref position, property, ref yPosition);

            // EFFECT //
            effectFoldoutGroup = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x, yPosition, position.width, lineHeight),
                effectFoldoutGroup, new GUIContent("Effect"));
            yPosition += lineSpacing;
            if (effectFoldoutGroup)
            {
                EditorGUI.indentLevel++;
                yPosition = DrawEffectGroup(position, property, yPosition);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        protected override float DrawGeneralGroup(ref Rect position, SerializedProperty property, float yPosition)
        {
            yPosition = base.DrawGeneralGroup(ref position, property, yPosition);
            EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("hitSoundbankName"));
            yPosition += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("hitSoundName"));
            yPosition += lineSpacing;
            return yPosition;
        }

        protected virtual float DrawEffectGroup(Rect position, SerializedProperty property, float yPosition)
        {
            EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("effectbankName"));
            yPosition += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("effectName"));
            yPosition += lineSpacing;
            return yPosition;
        }

        protected override float DrawForcesGroup(ref Rect position, SerializedProperty property, float yPosition)
        {
            float yPos = base.DrawForcesGroup(ref position, property, yPosition);
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentForceAir"), GUIContent.none);
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentFriction"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentGravity"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("holdVelocityTime"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("hangTime"));
            yPos += lineSpacing;
            return yPos;
        }
    }
}