using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MonoBehaviourCreator : EditorWindow
{
    private string _scriptName = "";
    private int _selectedFeatureIndex = 0;
    private string[] _featureFolders;

    [MenuItem("Tools/MonoBehaviour Creator")]
    public static void ShowWindow()
    {
        GetWindow<MonoBehaviourCreator>("MonoBehaviour Creator");
    }

    private void OnEnable()
    {
        LoadFeatureFolders();
    }

    private void LoadFeatureFolders()
    {
        string featuresRoot = "Assets/_/Features";
        if (Directory.Exists(featuresRoot))
        {
            _featureFolders = Directory.GetDirectories(featuresRoot);
            for (int i = 0; i < _featureFolders.Length; i++)
            {
                _featureFolders[i] = Path.GetFileName(_featureFolders[i]);
            }
        }
        else
        {
            _featureFolders = new string[0];
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Créer un MonoBehaviour personnalisé", EditorStyles.boldLabel);

        _scriptName = EditorGUILayout.TextField("Nom du script", _scriptName);

        if (_featureFolders.Length == 0)
        {
            EditorGUILayout.HelpBox("Aucun dossier 'Features' trouvé dans Assets/_/Features", MessageType.Warning);
        }
        else
        {
            _selectedFeatureIndex = EditorGUILayout.Popup("Choisir une Feature", _selectedFeatureIndex, _featureFolders);
        }

        if (GUILayout.Button("Créer le script"))
        {
            if (string.IsNullOrEmpty(_scriptName))
            {
                EditorUtility.DisplayDialog("Erreur", "Le nom du script est vide !", "OK");
                return;
            }

            if (_featureFolders.Length == 0)
            {
                EditorUtility.DisplayDialog("Erreur", "Aucun dossier features valide trouvé.", "OK");
                return;
            }

            CreateMonoBehaviourScript(_featureFolders[_selectedFeatureIndex], _scriptName);
        }
    }

    private void CreateMonoBehaviourScript(string featureName, string scriptName)
    {
        string scriptsPath = Path.Combine($"Assets/_/Features/{featureName}/Runtime/Scripts");

        if (!Directory.Exists(scriptsPath))
            Directory.CreateDirectory(scriptsPath);

        string scriptPath = Path.Combine(scriptsPath, $"{scriptName}.cs");

        if (File.Exists(scriptPath))
        {
            if (!EditorUtility.DisplayDialog("Écraser?", $"Le fichier {scriptName}.cs existe déjà. Écraser?", "Oui", "Non"))
                return;
        }

        string scriptContent = $@"
using UnityEngine;
using Foundation.Runtime;

namespace {featureName}.Runtime
{{
    public class {scriptName} : FBehaviour
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
        Debug.Log($"✅ Script {scriptName}.cs créé dans {scriptsPath}");
    }
}

