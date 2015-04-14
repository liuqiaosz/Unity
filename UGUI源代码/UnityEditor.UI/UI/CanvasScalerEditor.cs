using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(CanvasScaler), true)]
    [CanEditMultipleObjects]
    public class CanvasScalerEditor : Editor
    {
        SerializedProperty m_UiScaleMode;
        SerializedProperty m_ScaleFactor;
        SerializedProperty m_ReferenceResolution;
        SerializedProperty m_ScreenMatchMode;
        SerializedProperty m_MatchWidthOrHeight;
        SerializedProperty m_PhysicalUnit;
        SerializedProperty m_FallbackScreenDPI;
        SerializedProperty m_DefaultSpriteDPI;
        SerializedProperty m_DynamicPixelsPerUnit;
        SerializedProperty m_ReferencePixelsPerUnit;

        const int kSliderEndpointLabelsHeight = 12;

        private class Styles
        {
            public GUIContent matchContent;
            public GUIContent widthContent;
            public GUIContent heightContent;
            public GUIStyle leftAlignedLabel;
            public GUIStyle rightAlignedLabel;

            public Styles()
            {
                matchContent = new GUIContent("Match");
                widthContent = new GUIContent("Width");
                heightContent = new GUIContent("Height");

                leftAlignedLabel = new GUIStyle(EditorStyles.label);
                rightAlignedLabel = new GUIStyle(EditorStyles.label);
                rightAlignedLabel.alignment = TextAnchor.MiddleRight;
            }
        }
        private static Styles s_Styles;

        protected virtual void OnEnable()
        {
            m_UiScaleMode = serializedObject.FindProperty("m_UiScaleMode");
            m_ScaleFactor = serializedObject.FindProperty("m_ScaleFactor");
            m_ReferenceResolution = serializedObject.FindProperty("m_ReferenceResolution");
            m_ScreenMatchMode = serializedObject.FindProperty("m_ScreenMatchMode");
            m_MatchWidthOrHeight = serializedObject.FindProperty("m_MatchWidthOrHeight");
            m_PhysicalUnit = serializedObject.FindProperty("m_PhysicalUnit");
            m_FallbackScreenDPI = serializedObject.FindProperty("m_FallbackScreenDPI");
            m_DefaultSpriteDPI = serializedObject.FindProperty("m_DefaultSpriteDPI");
            m_DynamicPixelsPerUnit = serializedObject.FindProperty("m_DynamicPixelsPerUnit");
            m_ReferencePixelsPerUnit = serializedObject.FindProperty("m_ReferencePixelsPerUnit");
        }

        public override void OnInspectorGUI()
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            bool allAreRoot = true;
            bool showWorldDiffers = false;
            bool showWorld = ((target as CanvasScaler).GetComponent<Canvas>().renderMode == RenderMode.WorldSpace);
            for (int i = 0; i < targets.Length; i++)
            {
                CanvasScaler scaler = targets[i] as CanvasScaler;
                Canvas canvas = scaler.GetComponent<Canvas>();
                if (!canvas.isRootCanvas)
                {
                    allAreRoot = false;
                    break;
                }
                if (showWorld && canvas.renderMode != RenderMode.WorldSpace || !showWorld && canvas.renderMode == RenderMode.WorldSpace)
                {
                    showWorldDiffers = true;
                    break;
                }
            }

            if (!allAreRoot)
            {
                EditorGUILayout.HelpBox("Non-root Canvases will not be scaled.", MessageType.Warning);
                return;
            }

            serializedObject.Update();

            EditorGUI.showMixedValue = showWorldDiffers;
            EditorGUI.BeginDisabledGroup(showWorld || showWorldDiffers);
            if (showWorld || showWorldDiffers)
            {
                EditorGUILayout.Popup(m_UiScaleMode.displayName, 0, new string[] { "World" });
            }
            else
            {
                EditorGUILayout.PropertyField(m_UiScaleMode);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.showMixedValue = false;

            if (!showWorldDiffers && !(!showWorld && m_UiScaleMode.hasMultipleDifferentValues))
            {
                EditorGUILayout.Space();

                // World Canvas
                if (showWorld)
                {
                    EditorGUILayout.PropertyField(m_DynamicPixelsPerUnit);
                }
                // Constant pixel size
                else if (m_UiScaleMode.enumValueIndex == (int)CanvasScaler.ScaleMode.ConstantPixelSize)
                {
                    EditorGUILayout.PropertyField(m_ScaleFactor);
                }
                // Scale with screen size
                else if (m_UiScaleMode.enumValueIndex == (int)CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    EditorGUILayout.PropertyField(m_ReferenceResolution);
                    EditorGUILayout.PropertyField(m_ScreenMatchMode);
                    if (m_ScreenMatchMode.enumValueIndex == (int)CanvasScaler.ScreenMatchMode.MatchWidthOrHeight && !m_ScreenMatchMode.hasMultipleDifferentValues)
                    {
                        Rect r = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + kSliderEndpointLabelsHeight);
                        DualLabeledSlider(r, m_MatchWidthOrHeight, s_Styles.matchContent, s_Styles.widthContent, s_Styles.heightContent);
                    }
                }
                // Constant physical size
                else if (m_UiScaleMode.enumValueIndex == (int)CanvasScaler.ScaleMode.ConstantPhysicalSize)
                {
                    EditorGUILayout.PropertyField(m_PhysicalUnit);
                    EditorGUILayout.PropertyField(m_FallbackScreenDPI);
                    EditorGUILayout.PropertyField(m_DefaultSpriteDPI);
                }

                EditorGUILayout.PropertyField(m_ReferencePixelsPerUnit);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DualLabeledSlider(Rect position, SerializedProperty property, GUIContent mainLabel, GUIContent labelLeft, GUIContent labelRight)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            Rect pos = position;

            position.y += 12;
            position.xMin += EditorGUIUtility.labelWidth;
            position.xMax -= EditorGUIUtility.fieldWidth;

            GUI.Label(position, labelLeft, s_Styles.leftAlignedLabel);
            GUI.Label(position, labelRight, s_Styles.rightAlignedLabel);

            EditorGUI.PropertyField(pos, property, mainLabel);
        }
    }
}
