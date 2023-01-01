// ControllerGlpyhs.cs
using UnityEngine;
using System.Collections;
using Rewired;

namespace rwby {
    using Rewired.Data.Mapping;

    // This is a basic example showing one way of storing glyph data for Joysticks
    [CreateAssetMenu(fileName = "Controller Glyphs")]
    public class ControllerGlyphs : ScriptableObject{

        [SerializeField]
        private ControllerEntry[] controllers;

        [SerializeField] private GlyphEntry[] mouseGlyphs;

        //private static ControllerGlyphs Instance;

        public Sprite GetGlyph(System.Guid joystickGuid, int elementIdentifierId, AxisRange axisRange) {
            if(controllers == null) return null;

            // Try to find the glyph
            for(int i = 0; i < controllers.Length; i++) {
                if(controllers[i] == null) continue;
                if(controllers[i].joystick == null) continue; // no joystick assigned
                if(controllers[i].joystick.Guid != joystickGuid) continue; // guid does not match
                return controllers[i].GetGlyph(elementIdentifierId, axisRange);
            }

            return null;
        }

        [System.Serializable]
        private class ControllerEntry {
            public string name;
            // This must be linked to the HardwareJoystickMap located in Rewired/Internal/Data/Controllers/HardwareMaps/Joysticks
            public HardwareJoystickMap joystick;
            public GlyphEntry[] glyphs;

            public Sprite GetGlyph(int elementIdentifierId, AxisRange axisRange) {
                if(glyphs == null) return null;
                for(int i = 0; i < glyphs.Length; i++) {
                    if(glyphs[i] == null) continue;
                    if(glyphs[i].elementIdentifierId != elementIdentifierId) continue;
                    return glyphs[i].GetGlyph(axisRange);
                }
                return null;
            }
        }

        [System.Serializable]
        private class GlyphEntry {
            public int elementIdentifierId;
            public Sprite glyph;
            public Sprite glyphPos;
            public Sprite glyphNeg;

            public Sprite GetGlyph(AxisRange axisRange) {
                switch(axisRange) {
                    case AxisRange.Full: return glyph;
                    case AxisRange.Positive: return glyphPos != null ? glyphPos : glyph;
                    case AxisRange.Negative: return glyphNeg != null ? glyphNeg : glyph;
                }
                return null;
            }
        }
    }
}