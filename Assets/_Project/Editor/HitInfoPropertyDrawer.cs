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
            float val = lineValue * 4;

            int windowsOpen = 0;

            if (property.FindPropertyRelative("groundedFoldoutGroup").boolValue) windowsOpen++;
            if (property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue) windowsOpen++;
            
            if (property.FindPropertyRelative("hit").FindPropertyRelative("hitEffectbank").isExpanded) val += lineValue * 2;
            if (property.FindPropertyRelative("counterhit").FindPropertyRelative("hitEffectbank").isExpanded) val += lineValue * 2;
            if (property.FindPropertyRelative("hit").FindPropertyRelative("blockEffectbank").isExpanded) val += lineValue * 2;
            if (property.FindPropertyRelative("counterhit").FindPropertyRelative("blockEffectbank").isExpanded) val += lineValue * 2;
            if (property.FindPropertyRelative("hit").FindPropertyRelative("hitSoundbank").isExpanded) val += lineValue * 2;
            if (property.FindPropertyRelative("counterhit").FindPropertyRelative("hitSoundbank").isExpanded) val += lineValue * 2;
            
            val += lineValue * 38 * windowsOpen;
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
                property.FindPropertyRelative("groundedFoldoutGroup").boolValue, new GUIContent("Hit"));
            if (property.FindPropertyRelative("groundedFoldoutGroup").boolValue)
            {
                DrawTopGroup(ref position, property.FindPropertyRelative("hit"));
            }
            EditorGUI.EndFoldoutHeaderGroup();
            
            
            property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue = EditorGUI.BeginFoldoutHeaderGroup(
                new Rect(position.x, GetLineY(), position.width, lineHeight),
                property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue, new GUIContent("Counter Hit"));
            if (property.FindPropertyRelative("groundedCounterHitFoldoutGroup").boolValue)
            {
                DrawTopGroup(ref position, property.FindPropertyRelative("counterhit"));
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawTopGroup(ref Rect position, SerializedProperty property)
        {
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "GENERAL", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("damage"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("chipDamage"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("ignoreProration"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("noKill"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("unblockable"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundHitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("airHitState"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBounce"));
            if(property.FindPropertyRelative("groundBounce").boolValue)
                EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("groundBounceForce"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("wallBounce"));
            if(property.FindPropertyRelative("wallBounce").boolValue)
                EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("wallBounceForce"));
            
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "FORCES", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceType"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceRelation"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitForceRelationOffset"), new GUIContent("Offset"));
            switch ((HitboxForceType)property.FindPropertyRelative("hitForceType").enumValueIndex)
            {
                case HitboxForceType.SET:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("groundHitForce"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("aerialHitForce"));
                    break;
                case HitboxForceType.PULL:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushMultiplier"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushMaxDistance"));
                    break;
                case HitboxForceType.PUSH:
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushMultiplier"));
                    EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight),
                        property.FindPropertyRelative("pullPushMaxDistance"));
                    break;
            }
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("blockLift"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("autolink"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("autolinkPercentage"));
            
            
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "STUN", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("attackerHitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitstop"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("ignoreHitstunScaling"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("untech"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("blockstun"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("initialProration"));
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("comboProration"));
            
            EditorGUI.LabelField(new Rect(position.x, GetLineY(), position.width, lineHeight), "EFFECTS", EditorStyles.boldLabel);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitEffectbank"), true);
            if (property.FindPropertyRelative("hitEffectbank").isExpanded)
            {
                GetLineY();
                GetLineY();
            }
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitEffect"), true);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("blockEffectbank"), true);
            if (property.FindPropertyRelative("blockEffectbank").isExpanded)
            {
                GetLineY();
                GetLineY();
            }
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("blockEffect"), true);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitSoundbank"), true);
            if (property.FindPropertyRelative("hitSoundbank").isExpanded)
            {
                GetLineY();
                GetLineY();
            }
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitSound"), true);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("cameraShakeLength"), true);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("hitCameraShakeStrength"), true);
            EditorGUI.PropertyField(new Rect(position.x, GetLineY(), position.width, lineHeight), property.FindPropertyRelative("blockCameraShakeStrength"), true);
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