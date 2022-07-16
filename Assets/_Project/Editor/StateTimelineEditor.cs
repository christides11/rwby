using HnSF;
using UnityEditor;
using UnityEngine;

namespace rwby
{
    [CustomEditor(typeof(rwby.StateTimeline), true)]
    public class StateTimelineEditor : HnSF.StateTimelineEditor 
    {
        public override void OnInspectorGUI()
        {
            StateTimeline st = (StateTimeline)target;
            DrawDefaultInspector();
            if (GUILayout.Button("Open Editor"))
            {
                st.BuildStateVariablesIDMap();
                currentTimeline = StateTimelineEditorWindow.OpenWindow(st);
            }

            if (GUI.changed)
            {
                if (currentTimeline)
                {
                    currentTimeline.RefreshFrameBars();
                }
            }
        }
    }
}