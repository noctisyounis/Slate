using Foundation.Runtime;
using UnityEngine;

namespace UpperBar.Runtime
{
    public class UpperBarBehaviour : FBehaviour 
    {
        #region Publics
        
            public float hoverZoneHeight = 10f;
            public float barHeight = 10f;
            public float transitionSpeed = 10f;
        
        #endregion

        #region Unity API
        
            void Start()
            {
                currentY = -barHeight;
            }

            void Update()
            {
                if (Input.mousePosition.y >= Screen.height - hoverZoneHeight)
                    isVisible = true;
                else if (Input.mousePosition.y < Screen.height - barHeight - 10f)
                    isVisible = false;

                var targetY = isVisible ? 0f : -barHeight;
                currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * transitionSpeed);
            }

            void OnGUI()
            {
                GUI.depth = 0;

                GUILayout.BeginArea(new Rect(0, currentY, Screen.width, barHeight), GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Menu", GUILayout.Width(100)))
                    Debug.Log("Menu clicked");

                if (GUILayout.Button("Options", GUILayout.Width(100)))
                    Debug.Log("Options clicked");

                if (GUILayout.Button("Quitter", GUILayout.Width(100)))
                    Application.Quit();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
        
        #endregion

        #region Main Methods

   
        #endregion

        #region Utils

   
        #endregion


        #region Privates & Protected
        
            private bool isVisible;
            private float currentY;
        
        #endregion
    }
}