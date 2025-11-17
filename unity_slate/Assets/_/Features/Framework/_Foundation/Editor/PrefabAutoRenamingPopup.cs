using UnityEngine;
using UnityEditor;
using System;

namespace Foundation.Editor
{
    public class PrefabAutoRenamePopup : EditorWindow
    {
        private static Action onValidate;
        private static Action onCancel;

        private GUIStyle richLabel;
       
        private bool confirmed = false; // <--- IMPORTANT

        public static void Show(Action validate, Action cancel)
        {
            var window = CreateInstance<PrefabAutoRenamePopup>();

            onValidate = validate;
            onCancel = cancel;

            window.titleContent = new GUIContent("Invalid Prefab Name");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 320, 100);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            
            if (richLabel == null)
            {
                richLabel = new GUIStyle(EditorStyles.boldLabel);
                richLabel.richText = true;   // << indispensable
            }
            
            GUILayout.Label("The prefab name must follow the following pattern:\n(<color='green'>P_</color>NameOfYourPrefab)\nDo you want to auto rename it ? :", richLabel);
            
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("No"))
            {
                Close();
            }

            if (GUILayout.Button("Yes"))
            {
                onValidate?.Invoke();
                confirmed = true;
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
    }
}