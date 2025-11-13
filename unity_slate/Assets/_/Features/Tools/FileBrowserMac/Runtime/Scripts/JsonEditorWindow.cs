using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using UnityEngine;

namespace FileBrowserMac.Runtime
{
    [Serializable]
    public class JsonEditorWindow
    {
        public string FilePath { get; private set; }
        public string Title { get; private set; }
        public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();
        public bool ShowWindow { get; set; } = true;

        private Dictionary<string, string> editBuffers = new Dictionary<string, string>();

        public string RawJsonText { get; private set; } = "";

        public JsonEditorWindow(string filePath, string rawJson, Dictionary<string, string> data = null)
        {
            FilePath = filePath;
            Title = $"Json Editor - {Path.GetFileName(filePath)}";
            RawJsonText = rawJson ?? "";
            Data = data ?? new Dictionary<string, string>();

            // Initialize edit buffers
            foreach (var kv in Data)
                editBuffers[kv.Key] = kv.Value;
        }

        public void Draw()
        {
            if (!ShowWindow) return;

            ImGui.SetNextWindowSize(new Vector2(520, 420), ImGuiCond.Once);
            if (ImGui.Begin(Title, ImGuiWindowFlags.NoCollapse))
            {
                ImGui.Text($"Fichier : {FilePath}");
                ImGui.Separator();

                if (ImGui.Button("Sauvegarder Modifications"))
                {
                    SaveToFile();
                }
                ImGui.SameLine();
                if (ImGui.Button("Reload depuis disque"))
                {
                    ReloadFromFile();
                }
                ImGui.SameLine();
                if (ImGui.Button("Fermer"))
                {
                    ShowWindow = false;
                }

                ImGui.Separator();

                // Générer le JSON complet pour affichage readonly
                var serializable = new SerializableDictionary
                {
                    //keys = Data.Keys.ToList(),
                    //values = Data.Values.ToList()
                };
                
                //string jsonString = JsonUtility.ToJson(serializable, true);

                ImGui.Text("Content:");
                ImGui.BeginChild("Content", new Vector2(0, 120));
                string buffer = RawJsonText;
                ImGui.InputTextMultiline("##content", ref buffer, 4096, new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);
                ImGui.EndChild();

                ImGui.Separator();

                ImGui.Text("Clés / Valeurs :");
                ImGui.BeginChild("JsonEditorScroll", new Vector2(0, 200));

                foreach (var key in Data.Keys.ToList())
                {
                    ImGui.PushID(key);
                    ImGui.TextColored(new Vector4(0.8f, 0.9f, 1f, 1f), key);
                    ImGui.SameLine(250);

                    string current = editBuffers.ContainsKey(key) ? editBuffers[key] : Data[key];
                    string bufferVal = current;

                    ImGui.SetNextItemWidth(220);
                    if (ImGui.InputText("##val", ref bufferVal, 1024))
                    {
                        if (!editBuffers.ContainsKey(key) || editBuffers[key] != bufferVal)
                        {
                            editBuffers[key] = bufferVal;
                            Data[key] = bufferVal;
                        }
                    }

                    ImGui.PopID();
                }

                ImGui.Separator();
                ImGui.Text("Ajouter une nouvelle clé :");
                if (AddNewKeyUI())
                {
                    // clé ajoutée
                }

                ImGui.EndChild();
            }
            ImGui.End();
        }


        // UI pour ajouter une nouvelle clé
        private string newKeyBuffer = "";
        private string newValBuffer = "";
        private bool AddNewKeyUI()
        {
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("Clé", ref newKeyBuffer, 256);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("Valeur", ref newValBuffer, 256);
            ImGui.SameLine();
            if (ImGui.Button("Ajouter"))
            {
                if (!string.IsNullOrWhiteSpace(newKeyBuffer))
                {
                    if (!Data.ContainsKey(newKeyBuffer))
                    {
                        Data[newKeyBuffer] = newValBuffer ?? "";
                        editBuffers[newKeyBuffer] = newValBuffer ?? "";
                        newKeyBuffer = "";
                        newValBuffer = "";
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"La clé '{newKeyBuffer}' existe déjà.");
                    }
                }
            }
            return false;
        }

        public void ReloadFromFile()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                Debug.LogWarning("Fichier introuvable: " + FilePath);
                return;
            }

            string jsonText = File.ReadAllText(FilePath);
            var factList = JsonUtility.FromJson<FactList>(jsonText);

            if (factList != null && factList.Facts != null)
            {
                Data.Clear();
                foreach (var fact in factList.Facts)
                {
                    string valJson = JsonUtility.ToJson(fact.Value);
                    Data[fact.Key] = valJson;
                }

                editBuffers.Clear();
                foreach (var kv in Data)
                    editBuffers[kv.Key] = kv.Value;
            }
            else
            {
                Debug.LogWarning("Format JSON inattendu, liste Facts non trouvée.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur reload JSON: " + ex.Message);
        }
    }

    public void SaveToFile()
    {
        try
        {
            // Remonter dictionnaire Data en FactList
            var factList = new FactList { Facts = new List<Fact>() };
            foreach (var kv in Data)
            {
                FactValue valueObj = JsonUtility.FromJson<FactValue>(kv.Value);
                if (valueObj == null)
                {
                    // Si ce n’est pas un JSON valide, simplement créer un FactValue minimal
                    valueObj = new FactValue { Value = kv.Value, ValueType = "", IsPersistent = false };
                }
                factList.Facts.Add(new Fact { Key = kv.Key, Value = valueObj });
            }

            string jsonOut = JsonUtility.ToJson(factList, true);
            File.WriteAllText(FilePath, jsonOut);
            Debug.Log("JSON sauvegardé : " + FilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur de sauvegarde JSON : " + ex.Message);
        }
    }
        // Sauvegarde le dictionnaire dans le fichier JSON (format SerializableDictionary)
        // Edit: A voir si Fact System ne pourrais pas envoyer les données au chemin du projet ouvert.
        /*public void SaveToFile()
        {
            try
            {
                var serializable = new SerializableDictionary
                {
                    keys = Data.Keys.ToList(),
                    values = Data.Values.ToList()
                };
                string jsonOut = JsonUtility.ToJson(serializable, true);
                File.WriteAllText(FilePath, jsonOut);
                Debug.Log("JSON sauvegardé : " + FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur de sauvegarde JSON : " + ex.Message);
            }
        }*/

        // Recharge le fichier depuis le disque (remplace Data)
       /* public void ReloadFromFile()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Debug.LogWarning("Fichier introuvable lors du reload: " + FilePath);
                    return;
                }

                string jsonText = File.ReadAllText(FilePath);
                var serializable = JsonUtility.FromJson<SerializableDictionary>(jsonText);
                if (serializable != null)
                {
                    Data = serializable.ToDictionary();
                    editBuffers.Clear();
                    foreach (var kv in Data)
                        editBuffers[kv.Key] = kv.Value;
                }
                else
                {
                    Debug.LogWarning("Reload JSON : format inattendu (SerializableDictionary attendu).");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur reload JSON: " + ex.Message);
            }
        }*/
        
    }
    
    [Serializable]
    public class FactValue
    {
        public string Value;
        public string ValueType;
        public bool IsPersistent;
    }

    [Serializable]
    public class Fact
    {
        public string Key;
        public FactValue Value;
    }

    [Serializable]
    public class FactList
    {
        public List<Fact> Facts;
    }


}
