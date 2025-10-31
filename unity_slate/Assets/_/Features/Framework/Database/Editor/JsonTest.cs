using Database.Runtime;
using UnityEditor;
using UnityEngine;

namespace Database.Editor
{
    public class JsonTest : UnityEditor.EditorWindow
    {
        [MenuItem("Custom Tool/JsonTest")]
        public static void CreateWindow()
        {
            _window = EditorWindow.GetWindow(typeof(JsonTest), true, "Json Test Window");
            _window.Show();
        }

        private void OnGUI()
        {
            // GUILayout.Button("Hello");
            _jsonContent = GUILayout.TextArea($"{_jsonContent}");

            if (GUILayout.Button("Deserialize"))
            {
                // _output = JsonConvert.DeserializeObject<Dictionary<string, SerializableFact>>(_jsonContent);
                _output = JsonUtility.FromJson<SerializableSave>(_jsonContent);
            }
            if(_output != null) GUILayout.Label(_output.Facts.ToString());
            if(_output != null) GUILayout.Label(_output.Facts.Count.ToString());
        }

        private static EditorWindow _window;
        private static string _jsonContent = "{\n    \"Facts\": [\n        {\n            \"Key\": \"CounterTest/Int\",\n            \"Value\": {\n                \"Value\": \"508\",\n                \"ValueType\": \"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\n                \"IsPersistent\": true\n            }\n        }\n    ]\n}\n";
        private static SerializableSave _output;
    }
}