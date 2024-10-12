using UnityEngine;

namespace Units.Editor
{
    public abstract class UnitDrawer : UnityEditor.PropertyDrawer
    {
        private const float SelectorSize = 60;
        private const float PostfixOffset = 4;
        private const float PostfixAlpha = .8f;

        protected void DrawUnit(Rect position, UnityEditor.SerializedProperty property, GUIContent label, double[] powers, GUIContent[] postfixes, int defaultIdx)
        {
            var editorData = property.FindPropertyRelative("EditorData");
            property = property.FindPropertyRelative("f");
            
            UnityEditor.EditorGUI.BeginProperty(position, label, property); 
            
            position.width -= SelectorSize;

            int idx = Mathf.Clamp(editorData.intValue + defaultIdx, 0, powers.Length);
            double pow = powers[idx];


            UnityEditor.EditorGUI.BeginChangeCheck();
            float val = (float)(property.floatValue / pow);
            float newVal = (float)(pow * UnityEditor.EditorGUI.FloatField(position, label, val));
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                property.floatValue = (float)newVal;
            }
            

            if (Event.current.type == EventType.Repaint)
            {
                var defaultColor = GUI.color;
                var faintColor = GUI.color;
                faintColor.a = PostfixAlpha;
                GUI.color = faintColor;

                float x = position.x + UnityEditor.EditorGUIUtility.labelWidth + UnityEditor.EditorStyles.label.CalcSize(label).x;
                UnityEditor.EditorStyles.label.Draw(new Rect(position) { x = x + PostfixOffset }, postfixes[idx], false, false, false,
                    false);
                GUI.color = defaultColor;
            }
            
            var pos2 = position;
            pos2.x += position.width;
            pos2.width = SelectorSize;
            UnityEditor.EditorGUI.BeginChangeCheck();
            idx = UnityEditor.EditorGUI.Popup(pos2, idx, postfixes);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                editorData.intValue = idx - defaultIdx;
            }

            UnityEditor.EditorGUI.EndProperty();
        }
    }
}