using UnityEngine;

namespace Hypnos.Debug
{
    public class DebugGuiSettings : ScriptableObject
    {
        public GUIContent ClearContent;
        public GUIContent CollapseContent;
        public GUIContent ClearOnNewSceneContent;
        public GUIContent ShowTimeContent;
        public GUIContent ShowSceneContent;
        public GUIContent UserContent;
        public GUIContent ShowMemoryContent;
        public GUIContent SoftwareContent;
        public GUIContent DateContent;
        public GUIContent ShowFpsContent;
        public GUIContent InfoContent;
        public GUIContent SearchContent;
        public GUIContent CloseContent;

        public GUIContent BuildFromContent;
        public GUIContent SystemInfoContent;
        public GUIContent GraphicsInfoContent;
        public GUIContent BackContent;

        public GUIContent LogContent;
        public GUIContent WarningContent;
        public GUIContent ErrorContent;
        public GUIStyle BarStyle;
        public GUIStyle ButtonActiveStyle;

        public GUIStyle NonStyle;
        public GUIStyle LowerLeftFontStyle;
        public GUIStyle BackStyle;
        public GUIStyle EvenLogStyle;
        public GUIStyle OddLogStyle;
        public GUIStyle LogButtonStyle;
        public GUIStyle SelectedLogStyle;
        public GUIStyle SelectedLogFontStyle;
        public GUIStyle StackLabelStyle;
        public GUIStyle ScrollerStyle;
        public GUIStyle SearchStyle;
        public GUIStyle SliderBackStyle;
        public GUIStyle SliderThumbStyle;

        public GUISkin ToolbarScrollerSkin;
        public GUISkin LogScrollerSkin;
        public GUISkin GraphScrollerSkin;
    }
}