using Foundation.Runtime;
using UnityEngine;

namespace UpperBar.Runtime
{
    public class UpperBarBehaviour : FBehaviour 
    {
        #region Publics
        
            
        
        #endregion

        #region Unity API
        
            public void Start()
            {
                _currentY = -_barHeight;
            }

            public void Update()
            {
                if (Input.mousePosition.y >= Screen.height - _hoverZoneHeight)
                    _isVisible = true;
                else if (Input.mousePosition.y < Screen.height - _barHeight - 10f)
                    _isVisible = false;

                var targetY = _isVisible ? 0f : -_barHeight;
                _currentY = Mathf.Lerp(_currentY, targetY, Time.deltaTime * _transitionSpeed);
            }

            public void OnGUI()
            {
                if (!_styleReady)
                {
                    _textStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = _fontSize,
                        normal = { textColor = _textColor },
                        alignment = TextAnchor.MiddleCenter
                    };
                    _styleReady = true;
                }

                var prev = GUI.color;
                GUI.color = _backgroundColor;
                GUI.Box(new Rect(0, _currentY, Screen.width, _barHeight), GUIContent.none);
                GUI.color = prev;

                GUILayout.BeginArea(new Rect(0, _currentY, Screen.width, _barHeight));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                DrawButton("_", () => Debug.Log("Menu clicked"));
                GUILayout.Space(_spacing);
                DrawButton("☐", () => Debug.Log("Options clicked"));
                GUILayout.Space(_spacing);
                DrawButton("X", Application.Quit);
                GUILayout.Space(_spacing);

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            private void DrawButton(string text, System.Action onClick)
            {
                var size = _textStyle.CalcSize(new GUIContent(text));

                var rect = GUILayoutUtility.GetRect(size.x, size.y,
                    GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                GUI.Label(rect, text, _textStyle);

                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    onClick?.Invoke();
            }
        
        #endregion

        #region Main Methods

   
        #endregion

        #region Utils

   
        #endregion
        
        #region Privates & Protected
        
            private bool _isVisible;
            private bool _styleReady;
            private float _currentY;
            private float _hoverZoneHeight = 30f;
            private float _barHeight = 30f;
            private float _transitionSpeed = 10f;
            private float _spacing = 20f;
            private int _fontSize = 16;
            private Color _backgroundColor = new Color(0f, 0f, 0f, 1f);
            private Color _textColor = Color.white;
            private GUIStyle _iconStyle;
            private GUIStyle _textStyle;
        
        #endregion
    }
}