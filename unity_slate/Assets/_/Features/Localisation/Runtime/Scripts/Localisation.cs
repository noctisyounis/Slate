
using System;
using System.Collections.Generic;
using System.IO;
using Foundation.Runtime;
using ImGuiNET;
using Manager.Runtime;
using Newtonsoft.Json;
using UnityEngine;

namespace Localisation.Runtime
{
    public class Localisation
    {
        #region Main Methods
        
            public void DrawUI()
            {
                InitializeDefault();
                InitializeGroups();
                if (!_loaded)
                {
                    _loaded = true;
                    LoadLocalizationData();
                }
                
                WindowPosManager.RegisterWindow("Localization Window###LocalizationUniqueId");
                
                
                if (ImGui.Begin("Localization Window###LocalizationUniqueId", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.DockNodeHost))
                {
                    WindowPosManager.SyncWindowPosition("Localization Window###LocalizationUniqueId");
                    ImGui.Text("Localisation");
                    float leftWidth = 200f;

                    ImGui.BeginChild("LeftPanel", new Vector2(leftWidth, 0));
                    DrawHierarchyPanel();
                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("RightPanel", new Vector2(0, 0));
                    DrawContentPanel();
                    ImGui.EndChild();

                    ImGui.Separator();

                }

                ImGui.End();    
            }
        #endregion

        #region Draw ImGui
        
            private void DrawHierarchyPanel()
            {
                ImGui.Text("Groups");
                ImGui.Separator();
                for (int i = 0; i < _groups.Count; i++)
                {
                    ImGui.PushID(i);
                    if (ImGui.Selectable(_groups[i], _selectedGroup == i))
                    {
                        _selectedGroup = i;
                    }
                    ImGui.PopID();
                }

                if (ImGui.Button("New group"))
                {
                    string newGroupName = $"New group {_groups.Count}";
                    _groups.Add(newGroupName);

                    foreach (var lang in _languages)
                    {
                        if (!_localisationByLanguage.ContainsKey(lang))
                            _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();
                
                        if (!_localisationByLanguage[lang].ContainsKey(newGroupName))
                            _localisationByLanguage[lang][newGroupName] = new List<LocalisationData>();
                    }
                }
                
            }

            private void DrawLanguagesCombo()
            {
                if (ImGui.BeginCombo("##LanguageCombo", _languages[_selectedLanguage]))
                {
                    for (int i = 0; i < _languages.Count; i++)
                    {
                        bool isSelected = (_selectedLanguage == i);
                        if (ImGui.Selectable(_languages[i], isSelected))
                        {
                            _selectedLanguage = i;
                            //LoadLocalizationData();
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
            }
            
            private void DrawContentPanel()
            {
                if (_selectedGroup == -1)
                {
                    ImGui.Text("Select a group on the left.");
                    return;
                }
                
                var winSize = ImGui.GetWindowSize();
                var rightStartX = winSize.x;

                var cur = ImGui.GetCursorPos();
                
                ImGui.Text($"Content for: {_groups[_selectedGroup]}");
                ImGui.SameLine();
                
                cur = ImGui.GetCursorPos();
                
                ImGui.SetCursorPos(new Vector2(rightStartX-cur.x, cur.y));
                ImGui.SetNextItemWidth(100);
                if (ImGui.Button("Refresh"))
                {   
                    _showConfirmRefresh = true;
                }

                if (_showConfirmRefresh)
                {
                    if (ImGui.Begin("RefreshConfirmPopup"))
                    {
                        ImGui.Text("Confirm Refresh ?");

                        if (ImGui.Button("OK"))
                        {
                            LoadLocalizationData();
                            ImGui.CloseCurrentPopup();
                            _showConfirmRefresh = false;
                        }

                        ImGui.SameLine();

                        if (ImGui.Button("Cancel"))
                        {
                            ImGui.CloseCurrentPopup();
                            _showConfirmRefresh = false;
                            
                        }

                        ImGui.End();
                    }
               
                }

                
                ImGui.SameLine();
                
                ImGui.SetCursorPos(new Vector2(rightStartX-100, cur.y));
                ImGui.SetNextItemWidth(100);
                DrawLanguagesCombo();
                
                ImGui.Separator();

                if (ImGui.BeginTable("LocalizationTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
                {
                    string groupName = _groups[_selectedGroup];
                    
                    Debug.Log($"<color='purple'>'_locaBL count : {_localisationByLanguage.Count}</color>");
                    
                    List<LocalisationData> datas = _localisationByLanguage[_languages[_selectedLanguage]][groupName];
                    
                    Debug.Log($"<color='orange'>'Group count : {_localisationByGroup.Count}</color>");
                    
                    ImGui.TableSetupColumn("#");
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Expression");
                    ImGui.TableHeadersRow();
                    DrawLocalizationTable(datas);
                    ImGui.EndTable();
                    if (ImGui.Button("New Data"))
                    {
                        string groupN = _groups[_selectedGroup];
                        Debug.Log($"<color='magenta'>LocByLangCount : {_localisationByLanguage.Count}</color> ");

                        foreach (var lang in _languages)
                        {
                            if (!_localisationByLanguage.ContainsKey(lang))
                                _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();

                            if (!_localisationByLanguage[lang].ContainsKey(groupN))
                                _localisationByLanguage[lang][groupN] = new List<LocalisationData>();

                            _localisationByLanguage[lang][groupN].Add(new LocalisationData());
                        }
                        
                        SaveLocalizationData(_groups[_selectedGroup]);
                    }
                    winSize = ImGui.GetWindowSize();
                    rightStartX = winSize.x;

                    ImGui.SetCursorPos(new Vector2(rightStartX-100, cur.y));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.Button("Save"))
                    {
                        SaveLocalizationData(_groups[_selectedGroup]);
                    }
                }
            }


            private void DrawLocalizationTable(List<LocalisationData> datas)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.PushID(i * 4 + 0);
                    ImGui.Text(i.ToString());
                    ImGui.PopID();

                    ImGui.TableSetColumnIndex(1);
                    ImGui.PushID(i * 4 + 1);
                    ImGui.InputText("##uuid", ref datas[i].m_uuid, 256);
                    ImGui.PopID();

                    ImGui.TableSetColumnIndex(2);
                    ImGui.PushID(i * 4 + 2);
                    ImGui.InputText("##expression", ref datas[i].m_expression, 256);
                    ImGui.PopID();
                }
            }

        #endregion

        private void InitializeDefault()
        {
            foreach (var group in _defaultGroups)
            {
                if(!_groups.Contains(group)) _groups.Add(group);
            }

            foreach (var lang in _defaultLanguages)
            {
                if(!_languages.Contains(lang)) _languages.Add(lang);
                if(!_localisationByLanguage.ContainsKey(lang)) _localisationByLanguage.Add(lang, new Dictionary<string, List<LocalisationData>>());
            }
        }
        
        
        private void InitializeGroups()
        {
            foreach (var group in _defaultGroups)
            {
                if (!_groups.Contains(group))
                    _groups.Add(group);

                foreach (var lang in _languages)
                {
                    if (!_localisationByLanguage.ContainsKey(lang))
                        _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();
                
                    if (!_localisationByLanguage[lang].ContainsKey(group))
                        _localisationByLanguage[lang][group] = new List<LocalisationData>();
                }
            }
        }
        

        
        private void SaveLocalizationData(string groupName = "")
        {
            SynchronizeUUIDAcrossLanguages(groupName);
            
            foreach (var lang in _localisationByLanguage)
            {
                string fileName = $"LocalisationData.{lang.Key}.json";
                LocalisationFile file = new LocalisationFile();
                Debug.Log($"<color='cyan'>Language KVP: {lang.Key}</color>");
            
                foreach (var group in lang.Value)
                {
                    Debug.Log($"<color='yellow'>Group: {group.Key}</color>");

                    var copy = new List<LocalisationData>(group.Value);
                
                    file.groups.Add(new GroupData
                    {
                        groupName = group.Key,
                        entries = copy
                    });
                }

                string json = JsonConvert.SerializeObject(file, Formatting.Indented);
                File.WriteAllText(fileName, json);

            }
        }

        private void SynchronizeUUIDAcrossLanguages(string groupName)
        {
            if (!_localisationByLanguage.ContainsKey(_languages[_selectedLanguage])) 
                return;

            var referenceList = _localisationByLanguage[_languages[_selectedLanguage]][groupName];

            foreach (var lang in _languages)
            {
                if (!_localisationByLanguage.ContainsKey(lang))
                    _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();

                if (!_localisationByLanguage[lang].ContainsKey(groupName))
                    _localisationByLanguage[lang][groupName] = new List<LocalisationData>();

                var currentList = _localisationByLanguage[lang][groupName];

                int count = Math.Min(referenceList.Count, currentList.Count);

                for (int i = 0; i < count; i++)
                {
                    currentList[i].m_uuid = referenceList[i].m_uuid;
                }
            }
        }
        
        
        private void LoadLocalizationData()
        {
            //_localisationByLanguage.Clear();
            //_groups.Clear();

            foreach (var lang in _languages)
            {
                string fileName = $"LocalisationData.{lang}.json";

                if (!File.Exists(fileName))
                {
                    Debug.Log($"No file found for {lang}, skipping load.");
                    continue;
                }

                string json = File.ReadAllText(fileName);
                LocalisationFile file = JsonConvert.DeserializeObject<LocalisationFile>(json);

                var groupDict = new Dictionary<string, List<LocalisationData>>();

                foreach (var group in file.groups)
                {
                    Debug.Log($"<color='cyan'>Found group {group.groupName} and entries {group.entries.Count} for language {lang}</color>");

                    if (!_groups.Contains(group.groupName))
                        _groups.Add(group.groupName);

                    groupDict[group.groupName] = group.entries;
                }

                _localisationByLanguage[lang] = groupDict;
            }

            if (_selectedLanguage >= 0 && _selectedLanguage < _languages.Count)
            {
                string selectedLang = _languages[_selectedLanguage];
                if (_localisationByLanguage.ContainsKey(selectedLang))
                {
                    _localisationByGroup = _localisationByLanguage[selectedLang];
                }
                else
                {
                    _localisationByGroup = new Dictionary<string, List<LocalisationData>>();
                }
            }
            else
            {
                _localisationByGroup = new Dictionary<string, List<LocalisationData>>();
            }

            Debug.Log($"Loaded localisation data for languages: {string.Join(", ", _localisationByLanguage.Keys)}");
        }

        
        
        private bool _showConfirmRefresh;
        private bool _loaded = false;
        private int _selectedGroup = -1;
        private int _selectedLanguage = 0;
        private string[] _defaultGroups = {"Player","Quest","Weapons"};
        private string[] _defaultLanguages = {"French","English","Spanish"};
        
        private List<string> _groups = new List<string>();
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<LocalisationData>> _localisationByGroup 
            = new Dictionary<string, List<LocalisationData>>();
        private Dictionary<string, Dictionary<string, List<LocalisationData>>> _localisationByLanguage =  new Dictionary<string, Dictionary<string, List<LocalisationData>>>();

    }
    
   
    
    public class LocalisationData
    {
        public string m_uuid = "";
        public string m_expression = "";
        public override string ToString()
        {
            return  $"UUID: {m_uuid}, Expression: {m_expression}";
        }

    }


    [System.Serializable]
    public class LocalisationFile
    {
        public List<GroupData> groups = new List<GroupData>();
    }

    [System.Serializable]
    public class GroupData
    {
        public string groupName;
        public List<LocalisationData> entries = new List<LocalisationData>();
    }


}

