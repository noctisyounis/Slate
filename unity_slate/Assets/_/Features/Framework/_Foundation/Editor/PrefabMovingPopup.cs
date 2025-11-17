using UnityEngine;
using UnityEditor;
using System;

namespace Foundation.Editor
{
    public class PrefabMovingPopup : EditorWindow
    {

        #region Unity API
            private void OnGUI()
            {
                
                if (richLabel == null)
                {
                    richLabel = new GUIStyle(EditorStyles.boldLabel);
                    richLabel.richText = true;   // << indispensable
                }

                GUILayout.Label($"The prefab path doesn't match Assets/_/Database/Prefabs \nIt will be moved in there :\n <color='green'>{newPath}</color> :", richLabel);

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Ok"))
                {
                    Close();
                }
                GUILayout.EndHorizontal();
            }

            private void OnDestroy()
            {
                onValidate?.Invoke(newPath);
            }
        
        #endregion
        
        
        #region Main Methods
        
            public static void Show(string newPrefabPath,Action<string> validate)
            {
                var window = CreateInstance<PrefabMovingPopup>();
                window.newPath = newPrefabPath;

                onValidate = validate;
                    
                window.titleContent = new GUIContent("Invalid Prefab Path");
                window.position = new Rect(Screen.width / 2, Screen.height / 2, 320, 100);
                window.ShowUtility();
            }

        #endregion
        
        #region Privates

            private static Action<string> onValidate;

            private GUIStyle richLabel;
            private string newPath = "";

        #endregion
    }
}