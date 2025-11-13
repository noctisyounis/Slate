using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using UnityEngine;

namespace FileBrowserMac.Runtime
{ 
    [Serializable]
    public class JsonLoader
    {
        public string rootPath = "";

        private string[] projectFolders = new string[0];
        private int projectIndex = 0;

        private string[] jsonFiles = new string[0];
        private int fileIndex = 0;

        public List<JsonEditorWindow> OpenEditors { get; private set; } = new List<JsonEditorWindow>();

        public void DrawUI()
        {
            ImGui.Text("Json Loader");
            ImGui.Separator();
            
            ImGui.Text("Root folder (example : path/to/ProjectsFolder)");
            ImGui.InputText("##rootPath", ref rootPath, 1024);

            ImGui.SameLine();
            if (ImGui.Button("Browse"))
            {
                string picked = null;

                #if UNITY_STANDALONE_OSX
                    picked = FileBrowserMac.OpenFolderDialog("Chose a root folder", rootPath);
                #else
                    Debug.LogWarning("File browser non disponible sur cette plateforme.");
                #endif

                if (!string.IsNullOrEmpty(picked))
                {
                    rootPath = picked;
                    RefreshProjectFolders();
                }
            }

            if (ImGui.Button("Refresh Projects"))
            {
                RefreshProjectFolders();
            }

            ImGui.Separator();
            
            if (projectFolders.Length == 0)
            {
                ImGui.TextColored(new Vector4(1, 0.7f, 0.7f, 1), "No project folders found in current selection.");
            }
            else
            {
                var projNames = projectFolders.Select(p => Path.GetFileName(p)).ToArray();
                ImGui.Text("Projects:");
                if (ImGui.Combo("##projects", ref projectIndex, projNames, projNames.Length))
                {
                    RefreshJsonFiles();
                }

                ImGui.SameLine();
 

                ImGui.Separator();
                
                if (jsonFiles.Length == 0)
                {
                    ImGui.TextColored(new Vector4(1, 0.85f, 0.6f, 1), "Aucun fichier JSON trouvé dans Data/GameFacts pour ce projet.");
                }
                else
                {
                    var fileNames = jsonFiles.Select(f => Path.GetFileName(f)).ToArray();
                    ImGui.Text("JSON Files (Data/GameFacts):");
                    if (ImGui.Combo("##jsonfiles", ref fileIndex, fileNames, fileNames.Length))
                    {
                        
                    }

                    ImGui.SameLine();

                    ImGui.Separator();

                    // Load button
                    if (ImGui.Button("Load Selected JSON"))
                    {
                        string fullPath = jsonFiles[fileIndex];
                        LoadJsonFile(fullPath);
                    }
                }
            }
        }
        
        // Rafraîchit la liste des projets
        private void RefreshProjectFolders()
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                projectFolders = new string[0];
                return;
            }

            try
            {
                projectFolders = Directory.GetDirectories(rootPath);
                projectIndex = 0;
                RefreshJsonFiles();
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur RefreshProjectFolders: " + ex.Message);
                projectFolders = new string[0];
            }
        }

        // Rafraîchit la liste des JSON dans Data/GameFacts du projet sélectionné
        private void RefreshJsonFiles()
        {
            jsonFiles = new string[0];
            fileIndex = 0;

            if (projectFolders == null || projectFolders.Length == 0) return;

            var projectPath = projectFolders[Mathf.Clamp(projectIndex, 0, projectFolders.Length - 1)];
            var lookup = Path.Combine(projectPath, "Data", "GameFacts");

            if (!Directory.Exists(lookup))
            {
                jsonFiles = new string[0];
                return;
            }

            try
            {
                jsonFiles = Directory.GetFiles(lookup, "*.json", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur RefreshJsonFiles: " + ex.Message);
                jsonFiles = new string[0];
            }
        }

        private void LoadJsonFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning("Fichier JSON introuvable: " + path);
                    return;
                }

                string jsonText = File.ReadAllText(path);
                Debug.Log("JSON TEXT: " + jsonText);

                // Désérialisez en FactList
                var factList = JsonUtility.FromJson<FactList>(jsonText);
                Dictionary<string, string> dict = new Dictionary<string, string>();

                if (factList != null && factList.Facts != null)
                {
                    foreach (var fact in factList.Facts)
                    {
                        string valJson = JsonUtility.ToJson(fact.Value);
                        dict[fact.Key] = valJson;
                    }
                }
                else
                {
                    dict = TryParseSimpleJsonToDict(jsonText); // fallback si JSON différent
                }

                var editor = new JsonEditorWindow(path, jsonText, dict);
                OpenEditors.Add(editor);
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur LoadJsonFile: " + ex.Message);
            }
        }


        
        // Fallback simple parser : tente d'extraire paires "key":"value" (utile si le JSON simple ne suit pas SerializableDictionary)
        private Dictionary<string, string> TryParseSimpleJsonToDict(string jsonText)
        {
            var result = new Dictionary<string, string>();
            // Très simple : on cherche "key": "value" patterns
            try
            {
                // éliminer espaces / retours ligne pour faciliter
                var cleaned = jsonText.Replace("\r", "").Replace("\n", "").Trim();
                // naive split { ...key..." : "value"... }
                int i = 0;
                while (i < cleaned.Length)
                {
                    int q1 = cleaned.IndexOf('"', i);
                    if (q1 == -1) break;
                    int q2 = cleaned.IndexOf('"', q1 + 1);
                    if (q2 == -1) break;
                    string key = cleaned.Substring(q1 + 1, q2 - q1 - 1);

                    int colon = cleaned.IndexOf(':', q2);
                    if (colon == -1) break;
                    int vq1 = cleaned.IndexOf('"', colon);
                    if (vq1 == -1) break;
                    int vq2 = cleaned.IndexOf('"', vq1 + 1);
                    if (vq2 == -1) break;
                    string val = cleaned.Substring(vq1 + 1, vq2 - vq1 - 1);

                    if (!result.ContainsKey(key))
                        result[key] = val;

                    i = vq2 + 1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Fallback parse failed: " + ex.Message);
            }

            return result;
        }
    }
}
