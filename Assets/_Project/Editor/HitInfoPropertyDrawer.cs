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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineValue = EditorGUIUtility.singleLineHeight;
            float val = lineValue * 6;

            int windowsOpen = 0;

            if (property.FindPropertyRelative("groundedFoldoutGroup").boolValue) windowsOpen++;
            if (property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue) windowsOpen++;
            if (property.FindPropertyRelative("aerialFoldoutGroup").boolValue) windowsOpen++;
            if (property.FindPropertyRelative("aerialCounterHitFoldoutGroup").boolValue) windowsOpen++;

            val += lineValue * 16 * windowsOpen;
            return val;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }

        public override void DrawProperty(ref Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("ID"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitStateGroundedGroups"));
            
            
            property.FindPropertyRelative("groundedFoldoutGroup").boolValue = EditorGUI.BeginFoldoutHeaderGroup(
                new Rect(position.x, GetLineY(), position.width, lineHeight),
                property.FindPropertyRelative("groundedFoldoutGroup").boolValue, new GUIContent("Ground"));
            if (property.FindPropertyRelative("groundedFoldoutGroup").boolValue)
            {
                DrawTopGroup(ref position, property.FindPropertyRelative("groundGroup"));
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue = EditorGUI.BeginFoldoutHeaderGroup(
                new Rect(position.x, GetLineY(), position.width, lineHeight),
                property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue, new GUIContent("Ground (Counter)"));
            if (property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue)
            {
                DrawTopGroup(ref position, property.FindPropertyRelative("groundCounterHitGroup"));
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            property.FindPropertyRelative("aerialFoldoutGroup").boolValue = EditorGUI.BeginFoldoutHeaderGroup(
                new Rect(position.x, GetLineY(), position.width, lineHeight),
                property.FindPropertyRelative("aerialFoldoutGroup").boolValue, new GUIContent("Aerial"));
            if (property.FindPropertyRelative("aerialFoldoutGroup").boolValue)
            {
                DrawTopGroup(ref position, property.FindPropertyRelative("aerialGroup"));
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            property.FindPropertyRelative("aerialCounterHitFoldoutGroup").boolValue = EditorGUI.BeginFoldoutHeaderGroup(
                new Rect(position.x, GetLineY(), position.width, lineHeight),
                property.FindPropertyRelative("aerialCounterHitFoldoutGroup").boolValue, new GUIContent("Aerial (Counter)"));
            if (property.FindPropertyRelative("aerialCounterHitFoldoutGroup").boolValue)
            {
                DrawTopGroup(ref position, property.FindPropertyRelative("aerialCounterHitGroup"));
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            /*
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
            }*/
            
        }

        private void DrawTopGroup(ref Rect position, SerializedProperty property)
        {
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "GENERAL", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitKills"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBounces"));
            if(property.FindPropertyRelative("groundBounces").intValue > 0)
                EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBounceForcePercentage"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("wallBounces"));
            if(property.FindPropertyRelative("wallBounces").intValue > 0)
                EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("wallBounceForcePercentage"));
            
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "FORCES", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceType"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceRelation"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceRelationOffset"), new GUIContent("Offset"));
            switch ((HitboxForceType)property.FindPropertyRelative("hitForceType").enumValueIndex)
            {
                case HitboxForceType.SET:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("hitForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("hitGravity"));
                    break;
                case HitboxForceType.PULL:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushCurve"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushMaxDistance"));
                    break;
                case HitboxForceType.PUSH:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushCurve"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushMaxDistance"));
                    break;
            }
            
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "STUN", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("attackerHitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("blockstun"));
        }

        protected override void DrawGeneralGroup(ref Rect position, SerializedProperty property)
        {
        }

        private bool forcesGroundFoldoutGroup;
        private bool forcesAerialFoldoutGroup;
        private bool forcesGroundCounterHitFoldoutGroup;
        private bool forcesAerialCounterHitFoldoutGroup;
        protected virtual void DrawForcesGroup(Rect position, SerializedProperty property)
        {
            /*
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceType"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceRelation"));
            forcesGroundFoldoutGroup = EditorGUI.Foldout(new Rect(position.x, GetLineY(), position.width, lineHeight),
                forcesGroundFoldoutGroup, new GUIContent("Ground"));
            if (forcesGroundFoldoutGroup)
            {
                DrawForcesGroundStateGroup(position, property, "hitForceType", "groundHitForce", "groundHitGravity");
            }
            forcesGroundCounterHitFoldoutGroup = EditorGUI.Foldout(new Rect(position.x, GetLineY(), position.width, lineHeight),
                forcesGroundCounterHitFoldoutGroup, new GUIContent("Ground (CounterHit)"));
            if (forcesGroundCounterHitFoldoutGroup)
            {
                DrawForcesGroundStateGroup(position, property, "hitForceType", "groundCounterHitForce", "groundCounterHitGravity");
            }
            forcesAerialFoldoutGroup = EditorGUI.Foldout(new Rect(position.x, GetLineY(), position.width, lineHeight),
                forcesAerialFoldoutGroup, new GUIContent("Aerial"));
            if (forcesAerialFoldoutGroup)
            {
                DrawForcesGroundStateGroup(position, property, "hitForceType", "aerialHitForce", "aerialHitGravity");
            }
            forcesAerialCounterHitFoldoutGroup = EditorGUI.Foldout(new Rect(position.x, GetLineY(), position.width, lineHeight),
                forcesAerialCounterHitFoldoutGroup, new GUIContent("Aerial (CounterHit)"));
            if (forcesAerialCounterHitFoldoutGroup)
            {
                DrawForcesGroundStateGroup(position, property, "hitForceType", "aerialCounterHitForce", "aerialCounterHitGravity");
            }*/
        }

        protected virtual void DrawForcesGroundStateGroup(Rect position, SerializedProperty property, string forceType,  string hitForce,
            string hitGravity)
        {
            /*
            switch ((HitboxForceType)property.FindPropertyRelative(forceType).enumValueIndex)
            {
                case HitboxForceType.SET:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative(hitForce));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative(hitGravity));
                    break;
                case HitboxForceType.PULL:
                    break;
                case HitboxForceType.PUSH:
                    break;
            }*/
        }

        protected virtual void DrawStunGroup(Rect position, SerializedProperty property)
        {
            /*
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("attackerHitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("counterHitAddedHitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundHitstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialHitstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBlockstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("aerialBlockstun"));*/
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