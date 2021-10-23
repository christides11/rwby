using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomPropertyDrawer(typeof(CustomBoxGroup), true)]
    public class CustomBoxGroupPropertyDrawer : HnSF.Combat.HurtboxGroupPropertyDrawer
    {
        protected override float DrawHurtboxGroupGeneralProperties(Rect position, SerializedProperty property, float lineSpacing, float lineHeight, float yPosition)
        {
            yPosition = base.DrawHurtboxGroupGeneralProperties(position, property, lineSpacing, lineHeight, yPosition);
            return yPosition;
        }
    }
}