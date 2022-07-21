using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace rwby
{
    public class StateTimelineEditorWindow : HnSF.StateTimelineEditorWindow
    {
        public static StateTimelineEditorWindow OpenWindow(StateTimeline stateTimeline)
        {
            StateTimelineEditorWindow wnd = CreateWindow<StateTimelineEditorWindow>();
            wnd.titleContent = new GUIContent("State Timeline");
            wnd.minSize = new Vector2(400, 300);
            wnd.stateTimeline = stateTimeline;
            wnd.RefreshAll(true);
            return wnd;
        }

        /*
        public override void DataBarsDrawParentAndChildren(List<VisualElement> dbs, int dataID, ref int incr, HnSF.StateTimeline stateTimeline)
        {
            int index = stateTimeline.stateVariablesIDMap[dataID];

            if (stateTimeline.data[index].FrameRanges != null)
            {
                for (int j = 0; j < stateTimeline.data[index].FrameRanges.Length; j++)
                {
                    int framebarStart = stateTimeline.data[index].FrameRanges[j].x < 0
                        ? (stateTimeline.data[index].FrameRanges[j].x == -1 ? 1 : this.stateTimeline.totalFrames+1) 
                        : (int)stateTimeline.data[index].FrameRanges[j].x;
                    int framebarWidth = stateTimeline.data[index].FrameRanges[j].x < 0
                        ?  (stateTimeline.data[index].FrameRanges[j].x == -1 ? this.stateTimeline.totalFrames-1 : 0)
                        : (int)stateTimeline.data[index].FrameRanges[j].y -
                          (int)stateTimeline.data[index].FrameRanges[j].x;
                    mainFrameBarLabel.CloneTree(dbs[incr]);
                    var thisMainFrameBarLabel = dbs[incr].Query(name: mainFrameBarLabel.name).Build().Last();
                    thisMainFrameBarLabel.style.left = GetFrameWidth() * framebarStart;
                    thisMainFrameBarLabel.style.width = new StyleLength(GetFrameWidth() *
                                                                        (framebarWidth + 1));
                    thisMainFrameBarLabel.Q<Label>().text = (stateTimeline.data[index].FrameRanges[j].y + 1 -
                                                             stateTimeline.data[index].FrameRanges[j].x).ToString();
                }
            }

            if (stateTimeline.data[index].Children == null) return;
            for (int i = 0; i < stateTimeline.data[index].Children.Length; i++)
            {
                int childIndex = stateTimeline.stateVariablesIDMap[stateTimeline.data[index].Children[i]];
                incr++;
                DataBarsDrawParentAndChildren(dbs, stateTimeline.data[childIndex].ID, ref incr, stateTimeline);
            }
        }*/
    }
}