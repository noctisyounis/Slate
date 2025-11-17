using UnityEngine;
using UnityEditor;
using System;

namespace Foundation.Editor
{
    public class PrefabRenamePopup : EditorWindow
    {

        #region Unity API

            private void OnGUI()
            {
                
                if (richLabel == null)
                {
                    richLabel = new GUIStyle(EditorStyles.boldLabel);
                    richLabel.richText = true;
                }
                
                GUILayout.Label("The prefab name must follow the following pattern:\n(<color='green'>P_</color>NameOfYourPrefab) :", richLabel);
                newName = EditorGUILayout.TextField("Name :", newName);
                
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }

                if (GUILayout.Button("Confirm"))
                {
                    confirmed = true;
                    onValidate?.Invoke(newName);
                    Close();
                }
                GUILayout.EndHorizontal();
            }

            private void OnDestroy()
            {
                if (!confirmed)
                {
                    onCancel?.Invoke();
                }
            }

        #endregion

        #region Main Methods

            public static void Show(string currentName, Action<string> validate, Action cancel)
            {
                var window = CreateInstance<PrefabRenamePopup>();
                window.newName = currentName;

                onValidate = validate;
                onCancel = cancel;

                window.titleContent = new GUIContent("Invalid Prefab Name");
                window.position = new Rect(Screen.width / 2, Screen.height / 2, 320, 100);
                window.ShowUtility();
            }
            
        #endregion
        

        #region Privates
        
            private static Action<string> onValidate;
            private static Action onCancel;

            private GUIStyle richLabel;
            private string newName = "";
            private bool confirmed = false;
            
        #endregion
      
    }
}