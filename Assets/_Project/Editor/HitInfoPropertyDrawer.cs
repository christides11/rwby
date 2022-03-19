using UnityEngine;
using UnityEditor;
using System;
using HnSF.Combat;

namespace rwby
{
    [CustomPropertyDrawer(typeof(rwby.HitInfo), true)]
    public class HitInfoPropertyDrawer : HnSF.Combat.HitInfoBasePropertyDrawer
    {
        protected bool effectFoldoutGroup;
        protected bool forcesFoldoutGroup;
        protected bool stunFoldoutGroup;
        public override void DrawProperty(ref Rect position, SerializedProperty property)
        {
            base.DrawProperty(ref position, property);

            // FORCES //
            forcesFoldoutGroup = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x,GetLineY(), position.width, lineHeight),
                forcesFoldoutGroup, new GUIContent("Forces"));
            if (forcesFoldoutGroup)
            {
                EditorGUI.indentLevel++;
                DrawForcesGroup(position, property);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            // STUN //
            stunFoldoutGroup = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x,GetLineY(), position.width, lineHeight),
                stunFoldoutGroup, new GUIContent("Stun"));
            if (stunFoldoutGroup)
            {
                EditorGUI.indentLevel++;
                DrawStunGroup(position, property);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            // EFFECT //
            effectFoldoutGroup = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x,GetLineY(), position.width, lineHeight),
                effectFoldoutGroup, new GUIContent("Effect"));
            if (effectFoldoutGroup)
            {
                EditorGUI.indentLevel++;
                DrawEffectGroup(position, property);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        protected override void DrawGeneralGroup(ref Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("ID"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundHitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundCounterHitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialHitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialCounterHitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBounces"));
            if(property.FindPropertyRelative("groundBounces").intValue > 0)
                EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBounceForcePercentage"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("wallBounces"));
            if(property.FindPropertyRelative("wallBounces").intValue > 0)
                EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("wallBounceForcePercentage"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitKills"));
        }
        
        protected virtual void DrawForcesGroup(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("forceType"));
            switch ((HitboxForceType)property.FindPropertyRelative("forceType").enumValueIndex)
            {
                case HitboxForceType.SET:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("forceRelation"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundHitForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundCounterHitForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBlockForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialHitForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialCounterHitForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialBlockForce"));
                    break;
                case HitboxForceType.PULL:
                    break;
                case HitboxForceType.PUSH:
                    break;
            }
            /*
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentFriction"));
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentGravity"));*/
        }

        protected virtual void DrawStunGroup(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("attackerHitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("counterHitAddedHitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundHitstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialHitstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBlockstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialBlockstun"));
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("blockHitstopAttacker"), new GUIContent("Block Hitstop (Attacker)", "Block Hitstop (Attacker)"));
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("blockHitstopDefender"), new GUIContent("Block Hitstop (Defender)", "Block Hitstop (Defender)"));
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("blockstun"), new GUIContent("Blockstun", "Blockstun"));
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("blockForce"), new GUIContent("Block Pushback", "Block Pushback"));
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("blockForceAir"), new GUIContent("Block Pushback (Air)", "Block Pushback (Air)"));
        }

        protected virtual void DrawEffectGroup(Rect position, SerializedProperty property)
        {
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("effectbankName"));
            //EditorGUI.PropertyField(new Rect(position.x, yPosition, position.width, lineHeight), property.FindPropertyRelative("effectName"));
        }
    }
}