using Foundation.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation.Editor
{
    [CustomEditor(typeof(FBehaviour), true)]
    [CanEditMultipleObjects]
    public class CustomInspector : UnityEditor.Editor
    {
        private FBehaviour _behaviour;
        private Color _debugColor;
        private Color _warningColor;
        private Color _errorColor;
        private Color _setColor;
        
        private void OnEnable()
        {
            _behaviour = target as FBehaviour;
            
            ColorUtility.TryParseHtmlString("#00CFFFC3", out _debugColor);
            ColorUtility.TryParseHtmlString("#FFE629", out _warningColor);
            ColorUtility.TryParseHtmlString("#FE4E54E4", out _errorColor);
            ColorUtility.TryParseHtmlString("#CAFF69ED", out _setColor);
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            _behaviour.m_debug = DebugTypeButton("Debug", _behaviour.m_debug, _debugColor);
            _behaviour.m_warning = DebugTypeButton("Warning", _behaviour.m_warning, _warningColor);
            _behaviour.m_error = DebugTypeButton("Error", _behaviour.m_error, _errorColor);
            //todo: create "All" button to set all to true or false
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            
            // base.OnInspectorGUI();
        }

        #region Utils

        private bool DebugTypeButton(string label, bool value, Color buttonColor)
        {
            GUI.backgroundColor = value ? _setColor : buttonColor;
            if (GUILayout.Button(value ? $"{label} ✓" : $"{label} X"))
            {
                value = !value;
            }
            
            GUI.backgroundColor = Color.white;
            
            return value;
        }

        #endregion
    }
}