using UnityEditor;
using UnityEngine;
using System.IO;

public class FeatureCreator : EditorWindow
{
    private string _featureName = "";

    [MenuItem("Tools/Feature Creator")]
    public static void ShowWindow()
    {
        GetWindow<FeatureCreator>("Feature Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Créer une nouvelle Feature", EditorStyles.boldLabel);

        _featureName = EditorGUILayout.TextField("Nom de la feature :", _featureName);

        if (GUILayout.Button("Créer la feature"))
        {
            if (string.IsNullOrEmpty(_featureName))
            {
                EditorUtility.DisplayDialog("Erreur", "Le nom de la feature est vide !", "OK");
                return;
            }

            CreateFeature(_featureName);
        }
    }

    private void CreateFeature(string name)
    {
        string root = $"Assets/_/Features/{name}";
        string runtimePath = $"{root}/Runtime";
        string scriptsPath = $"{runtimePath}/Scripts";

        Directory.CreateDirectory(scriptsPath);

        // Créer le fichier Assembly Definition
        string asmdefPath = $"{runtimePath}/{name}.Runtime.asmdef";
        string asmdefContent = $@"{{
    ""name"": ""{name}.Runtime"",
    ""rootNamespace"": ""{name}.Runtime"",
    ""references"": [
        ""Foundation.Runtime""
],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""autoReferenced"": true,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        File.WriteAllText(asmdefPath, asmdefContent);

        // Créer un script exemple
        string scriptPath = $"{scriptsPath}/{name}Behaviour.cs";
        string scriptContent = $@"
using Foundation.Runtime;
namespace {name}.Runtime
{{
    public class {name}Behaviour : FBehaviour 
    {{
        #region Public

   
        #endregion

        #region Unity API

   
        #endregion

        #region Main Methods

   
        #endregion

        #region Utils

   
        #endregion

        #region Private & Protected

   
        #endregion
    }}
}}";
        File.WriteAllText(scriptPath, scriptContent);

        AssetDatabase.Refresh();
        Debug.Log($"✅ Feature '{name}' créée avec succès !");
    }
}